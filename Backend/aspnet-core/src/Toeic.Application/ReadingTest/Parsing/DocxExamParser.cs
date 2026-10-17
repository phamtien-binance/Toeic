using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using Toeic.ReadingTest.Dto;

namespace Toeic.ReadingTest.Parsing
{
    /// <summary>
    /// Reads the HERMES-EXAM-TAG 1.0 contract out of a .docx file.
    /// The parser never throws on bad content: it collects ImportValidationError items
    /// so the caller can reject the whole file and show the teacher what to fix.
    /// </summary>
    public class DocxExamParser
    {
        private static readonly Regex TagRx = new Regex(@"^\[\[\s*/?\s*[A-Z_]+\b[^\]]*\]\]$", RegexOptions.Compiled);
        private static readonly Regex AttrRx = new Regex(@"([a-zA-Z_]+)\s*=\s*""([^""]*)""", RegexOptions.Compiled);
        private static readonly Regex QNumRx = new Regex(@"^\s*(\d{1,3})\s*[\.\)]", RegexOptions.Compiled);

        public class ParsedExam
        {
            public string Title { get; set; }
            public int TotalQuestions { get; set; }
            public string TestName { get; set; }
            public List<ParsedPart> Parts { get; } = new List<ParsedPart>();
            public List<ImportValidationError> Errors { get; } = new List<ImportValidationError>();
        }

        public class ParsedPart
        {
            public int Number { get; set; }
            public List<ParsedPassage> Passages { get; } = new List<ParsedPassage>();
            public List<ParsedQuestion> StandaloneQuestions { get; } = new List<ParsedQuestion>();
            public List<ParsedQuestion> AllQuestions()
            {
                return Passages.SelectMany(p => p.Questions).Concat(StandaloneQuestions).ToList();
            }
        }

        public class ParsedPassage
        {
            public string Id { get; set; }
            public int Part { get; set; }
            public string Type { get; set; }
            public string Header { get; set; }
            public int OrderIndex { get; set; }
            public string GroupCode { get; set; }
            public List<ParsedDocument> Documents { get; } = new List<ParsedDocument>();
            public List<ParsedQuestion> Questions { get; } = new List<ParsedQuestion>();
        }

        public class ParsedDocument
        {
            public string Label { get; set; }
            public string Type { get; set; }
            public string Text { get; set; }
            public string ImageSrc { get; set; }
        }

        public class ParsedQuestion
        {
            public int Number { get; set; }
            public int Part { get; set; }
            public string Stem { get; set; }
            public string AnswerKey { get; set; }
            public string Hint { get; set; }
            public int OrderIndex { get; set; }
            public Dictionary<string, string> Options { get; } = new Dictionary<string, string>();
        }

        public ParsedExam Parse(Stream docx)
        {
            var lines = ReadLines(docx);
            var exam = new ParsedExam();

            ParsedPart part = null;
            ParsedPassage passage = null;
            ParsedPassage groupPassage = null;
            ParsedQuestion question = null;
            ParsedDocument document = null;
            bool skipping = false;
            int order = 0;
            var pendingPassageDoc = new StringBuilder();

            foreach (var raw in lines)
            {
                var line = raw.Trim();
                if (line.Length == 0) continue;

                if (line.StartsWith("[[SPEC") || line.StartsWith("[[ANSWER_SHEET"))
                {
                    skipping = true; continue;
                }
                if (line.StartsWith("[[/SPEC") || line.StartsWith("[[/ANSWER_SHEET"))
                {
                    skipping = false; continue;
                }
                if (skipping) continue;

                if (line.StartsWith("[[DOC"))
                {
                    var a = Attrs(line);
                    exam.TestName = TryGet(a, "test");
                    continue;
                }
                if (line.StartsWith("[[META"))
                {
                    var a = Attrs(line);
                    if (TryGet(a, "key") == "title") exam.Title = TryGet(a, "value");
                    continue;
                }
                if (line.StartsWith("[[GROUP"))
                {
                    groupPassage = null;
                    question = null;
                    continue;
                }
                if (line.StartsWith("[[PART"))
                {
                    var a = Attrs(line);
                    part = new ParsedPart { Number = ToInt(TryGet(a, "n"), exam.Parts.Count + 5) };
                    exam.Parts.Add(part);
                    continue;
                }
                if (line.StartsWith("[[/PART")) { part = null; continue; }
                if (line.StartsWith("[[/GROUP")) { passage = null; groupPassage = null; question = null; continue; }
                if (line.StartsWith("[[/PASSAGE")) { passage = null; document = null; continue; }
                if (line.StartsWith("[[/Q")) { question = null; continue; }

                if (line.StartsWith("[[PASSAGE"))
                {
                    if (part == null)
                    {
                        exam.Errors.Add(Err("PASSAGE_NO_PART", "Thẻ PASSAGE nằm ngoài PART.", null));
                        continue;
                    }
                    var a = Attrs(line);
                    passage = new ParsedPassage
                    {
                        Id = TryGet(a, "id"),
                        Part = part.Number,
                        Type = TryGet(a, "type") ?? "article",
                        OrderIndex = part.Passages.Count
                    };
                    part.Passages.Add(passage);
                    groupPassage = passage;
                    continue;
                }
                if (line.StartsWith("[[IMAGE"))
                {
                    var a = Attrs(line);
                    if (passage != null)
                    {
                        passage.Documents.Add(new ParsedDocument
                        {
                            Label = (TryGet(a, "alt") ?? "image") + " — " + (TryGet(a, "src") ?? ""),
                            Type = "image",
                            ImageSrc = TryGet(a, "src")
                        });
                    }
                    continue;
                }
                if (line.StartsWith("[[Q"))
                {
                    if (part == null)
                    {
                        exam.Errors.Add(Err("Q_NO_PART", "Thẻ Q nằm ngoài PART.", null));
                        continue;
                    }
                    var a = Attrs(line);
                    order++;
                    question = new ParsedQuestion
                    {
                        Number = ToInt(TryGet(a, "n"), order + 100),
                        Part = ToInt(TryGet(a, "part"), part.Number),
                        OrderIndex = order
                    };
                    if (groupPassage != null) groupPassage.Questions.Add(question);
                    else part.StandaloneQuestions.Add(question);
                    continue;
                }

                // single-line OPT / ANS / HINT
                var optMatch = Regex.Match(line, @"^\[\[OPT\s+key=""([A-Da-d])""\]\](.*)\[\[/OPT\]\]$");
                if (optMatch.Success && question != null)
                {
                    question.Options[optMatch.Groups[1].Value.ToUpperInvariant()] = optMatch.Groups[2].Value.Trim();
                    continue;
                }
                var ansMatch = Regex.Match(line, @"^\[\[ANS\s+key=""([A-Da-d])""\]\](.*)\[\[/ANS\]\]$");
                if (ansMatch.Success && question != null)
                {
                    question.AnswerKey = ansMatch.Groups[1].Value.ToUpperInvariant();
                    continue;
                }
                var hintMatch = Regex.Match(line, @"^\[\[HINT\b[^\]]*\]\](.*)\[\[/HINT\]\]$");
                if (hintMatch.Success && question != null)
                {
                    question.Hint = hintMatch.Groups[1].Value.Trim();
                    continue;
                }

                if (TagRx.IsMatch(line)) continue; // unknown tag: tolerate

                // plain content
                if (question != null)
                {
                    var text = StripNumber(line, question.Number);
                    question.Stem = string.IsNullOrWhiteSpace(question.Stem) ? text : question.Stem + "\n" + text;
                }
                else if (passage != null)
                {
                    if (line.StartsWith("Questions ") && line.Contains("refer to")) passage.Header = line;
                    else passage.Documents.Add(new ParsedDocument { Label = null, Type = "text", Text = line });
                }
            }

            exam.TotalQuestions = exam.Parts.Sum(p => p.AllQuestions().Count);
            Validate(exam);
            return exam;
        }

        private void Validate(ParsedExam exam)
        {
            if (exam.Parts.Count == 0)
                exam.Errors.Add(Err("NO_PART", "Không tìm thấy thẻ PART nào trong file.", null));

            var all = exam.Parts.SelectMany(p => p.AllQuestions()).OrderBy(q => q.Number).ToList();
            if (all.Count == 0)
                exam.Errors.Add(Err("NO_QUESTION", "File không có câu hỏi nào.", null));

            var seen = new HashSet<int>();
            foreach (var q in all)
            {
                if (!seen.Add(q.Number))
                    exam.Errors.Add(Err("DUPLICATE_NUMBER", $"Số câu {q.Number} bị lặp.", q.Number));

                if (q.Options.Count != 4)
                    exam.Errors.Add(Err("OPTION_COUNT", $"Câu {q.Number}: cần đúng 4 phương án, tìm thấy {q.Options.Count}.", q.Number));

                if (string.IsNullOrEmpty(q.AnswerKey))
                    exam.Errors.Add(Err("MISSING_ANS", $"Câu {q.Number}: thiếu thẻ đáp án (ANS).", q.Number));
                else if (!q.Options.ContainsKey(q.AnswerKey))
                    exam.Errors.Add(Err("ANS_NOT_IN_OPTIONS", $"Câu {q.Number}: đáp án {q.AnswerKey} không có trong các phương án.", q.Number));

                if (string.IsNullOrWhiteSpace(q.Hint))
                    exam.Errors.Add(Err("MISSING_HINT", $"Câu {q.Number}: thiếu gợi ý (HINT).", q.Number));

                if (string.IsNullOrWhiteSpace(q.Stem))
                    exam.Errors.Add(Err("MISSING_STEM", $"Câu {q.Number}: thiếu nội dung câu hỏi.", q.Number));
            }

            // sequential numbering 101..N
            if (all.Count > 0)
            {
                var expected = all[0].Number;
                foreach (var q in all)
                {
                    if (q.Number != expected)
                    {
                        exam.Errors.Add(Err("BAD_SEQUENCE", $"Sai thứ tự câu: mong đợi {expected}, tìm thấy {q.Number}.", q.Number));
                        break;
                    }
                    expected++;
                }
            }

            if (exam.TotalQuestions > 0 && all.Count > 0 && all[0].Number != 101)
                exam.Errors.Add(Err("BAD_START", $"Câu đầu tiên phải là 101, tìm thấy {all[0].Number}.", all[0].Number));
        }

        private static string StripNumber(string line, int number)
        {
            var m = QNumRx.Match(line);
            if (m.Success && m.Groups[1].Value.Replace(" ", "") == number.ToString())
                return line.Substring(m.Length).Trim();
            return line;
        }

        private static int ToInt(string s, int fallback)
        {
            return int.TryParse(s, out var v) ? v : fallback;
        }

        private static ImportValidationError Err(string code, string msg, int? n)
        {
            return new ImportValidationError { Code = code, Message = msg, QuestionNumber = n };
        }

        private static string TryGet(Dictionary<string, string> d, string key)
        {
            return d != null && d.TryGetValue(key, out var v) ? v : null;
        }

        private static Dictionary<string, string> Attrs(string line)
        {
            var d = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (Match m in AttrRx.Matches(line)) d[m.Groups[1].Value] = m.Groups[2].Value;
            return d;
        }

        private static List<string> ReadLines(Stream stream)
        {
            var lines = new List<string>();
            using var doc = WordprocessingDocument.Open(stream, false);
            var body = doc.MainDocumentPart?.Document?.Body;
            if (body == null) return lines;

            foreach (var el in body.Descendants<Paragraph>())
            {
                var text = string.Concat(el.Descendants<Text>().Select(t => t.Text));
                if (string.IsNullOrWhiteSpace(text)) continue;
                // a paragraph may hold several tag lines separated by breaks
                foreach (var chunk in text.Split('\n'))
                    lines.Add(chunk);
            }
            return lines;
        }
    }
}
