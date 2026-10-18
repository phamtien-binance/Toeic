using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.UI;
using Toeic.Authorization;
using Toeic.ReadingTest.Dto;
using Toeic.ReadingTest.Parsing;

namespace Toeic.ReadingTest
{
    public interface IExamAppService : IApplicationService
    {
        Task<List<ExamListDto>> GetAllAsync(GetExamsInput input);
        Task<ExamDetailDto> GetDetailAsync(long id);
        Task<ImportResultDto> ImportFromDocxAsync(ImportExamInput input);
        Task PublishAsync(ExamIdInput input);
        Task UnpublishAsync(ExamIdInput input);
        Task DeleteAsync(ExamIdInput input);
        Task UpdateQuestionAsync(UpdateQuestionInput input);
        Task<ExamStatsDto> GetStatsAsync(long id);
        Task<List<StudentExamDto>> GetStudentExamsAsync(StudentExamsInput input);
        Task<List<StudentAttemptDto>> GetStudentAttemptsAsync(StudentAttemptsInput input);
    }

    public class ExamAppService : ToeicAppServiceBase, IExamAppService
    {
        private readonly IRepository<Exam, long> _examRepo;
        private readonly IRepository<Passage, long> _passageRepo;
        private readonly IRepository<PassageDocument, long> _documentRepo;
        private readonly IRepository<Question, long> _questionRepo;
        private readonly IRepository<QuestionOption, long> _optionRepo;
        private readonly IRepository<Attempt, long> _attemptRepo;
        private readonly IRepository<AttemptAnswer, long> _answerRepo;
        private readonly string _uploadsRoot;

        public ExamAppService(
            Microsoft.Extensions.Configuration.IConfiguration configuration,
            IRepository<Exam, long> examRepo,
            IRepository<Passage, long> passageRepo,
            IRepository<PassageDocument, long> documentRepo,
            IRepository<Question, long> questionRepo,
            IRepository<QuestionOption, long> optionRepo,
            IRepository<Attempt, long> attemptRepo,
            IRepository<AttemptAnswer, long> answerRepo)
        {
            _examRepo = examRepo;
            _passageRepo = passageRepo;
            _documentRepo = documentRepo;
            _questionRepo = questionRepo;
            _optionRepo = optionRepo;
            _attemptRepo = attemptRepo;
            _answerRepo = answerRepo;
            _uploadsRoot = configuration["Uploads:Root"] ?? Path.Combine(Path.GetTempPath(), "toeic-uploads");
        }

        // ---------------------------------------------------------------- list
        [AbpAuthorize(PermissionNames.Pages_Exam)]
        public async Task<List<ExamListDto>> GetAllAsync(GetExamsInput input)
        {
            var exams = await _examRepo.GetAllListAsync();
            var attempts = await _attemptRepo.GetAllListAsync();

            var query = exams.AsQueryable();
            if (input != null && !string.IsNullOrWhiteSpace(input.Keyword))
                query = query.Where(e => e.Title != null && e.Title.Contains(input.Keyword, StringComparison.OrdinalIgnoreCase));
            if (input != null && !string.IsNullOrWhiteSpace(input.Status) &&
                Enum.TryParse<ExamStatus>(input.Status, true, out var st))
                query = query.Where(e => e.Status == st);

            return query
                .OrderByDescending(e => e.CreationTime)
                .AsEnumerable()
                .Select(e =>
                {
                    var mine = attempts.Where(a => a.ExamId == e.Id).ToList();
                    return new ExamListDto
                    {
                        Id = e.Id,
                        Title = e.Title,
                        Description = e.Description,
                        DurationMinutes = e.DurationMinutes,
                        TotalQuestions = e.TotalQuestions,
                        Status = e.Status.ToString(),
                        SourceFile = e.SourceFile,
                        PublishedAt = e.PublishedAt,
                        OpenAt = e.OpenAt,
                        CloseAt = e.CloseAt,
                        CreationTime = e.CreationTime,
                        AttemptCount = mine.Count,
                        AverageScore = mine.Any() ? Math.Round(mine.Average(a => a.Score), 1) : 0
                    };
                }).ToList();
        }

        [AbpAuthorize(PermissionNames.Pages_Exam)]
        public async Task<ExamDetailDto> GetDetailAsync(long id)
        {
            var exam = await _examRepo.GetAsync(id);
            var passages = (await _passageRepo.GetAllListAsync()).Where(p => p.ExamId == id).OrderBy(p => p.OrderIndex).ToList();
            var documents = (await _documentRepo.GetAllListAsync()).Where(d => passages.Any(p => p.Id == d.PassageId)).ToList();
            var questions = (await _questionRepo.GetAllListAsync()).Where(q => q.ExamId == id).OrderBy(q => q.OrderIndex).ToList();
            var options = (await _optionRepo.GetAllListAsync()).Where(o => questions.Any(q => q.Id == o.QuestionId)).ToList();

            QuestionDto Map(Question q) => new QuestionDto
            {
                Id = q.Id,
                Number = q.Number,
                Part = q.Part,
                PassageId = q.PassageId,
                Stem = q.Stem,
                AnswerKey = q.AnswerKey,
                Hint = q.Hint,
                Options = options.Where(o => o.QuestionId == q.Id).OrderBy(o => o.OrderIndex)
                    .Select(o => new OptionDto { Key = o.OptionKey, Text = o.Text, OrderIndex = o.OrderIndex }).ToList()
            };

            return new ExamDetailDto
            {
                Id = exam.Id,
                Title = exam.Title,
                Description = exam.Description,
                DurationMinutes = exam.DurationMinutes,
                TotalQuestions = exam.TotalQuestions,
                Status = exam.Status.ToString(),
                SourceFile = exam.SourceFile,
                ShuffleQuestions = exam.ShuffleQuestions,
                ShuffleOptions = exam.ShuffleOptions,
                Passages = passages.Select(p => new PassageDto
                {
                    Id = p.Id,
                    GroupCode = p.GroupCode,
                    Part = p.Part,
                    PassageType = p.PassageType,
                    Header = p.Header,
                    OrderIndex = p.OrderIndex,
                    Documents = documents.Where(d => d.PassageId == p.Id).OrderBy(d => d.OrderIndex)
                        .Select(d => new PassageDocumentDto
                        {
                            Id = d.Id,
                            Label = d.Label,
                            DocumentType = d.DocumentType,
                            Text = d.Text,
                            ImagePath = d.ImagePath,
                            OrderIndex = d.OrderIndex
                        }).ToList(),
                    Questions = questions.Where(q => q.PassageId == p.Id).Select(Map).ToList()
                }).ToList(),
                StandaloneQuestions = questions.Where(q => q.PassageId == null).Select(Map).ToList()
            };
        }

        // ---------------------------------------------------------------- import
        [AbpAuthorize(PermissionNames.Pages_Exam)]
        public async Task<ImportResultDto> ImportFromDocxAsync(ImportExamInput input)
        {
            if (input == null || string.IsNullOrWhiteSpace(input.ContentBase64))
                throw new UserFriendlyException("Chưa có nội dung file .docx.");

            byte[] bytes;
            try
            {
                bytes = Convert.FromBase64String(input.ContentBase64);
            }
            catch (FormatException)
            {
                throw new UserFriendlyException("File gửi lên không phải base64 hợp lệ.");
            }

            ParsedExamParserResult parsed;
            using (var ms = new MemoryStream(bytes))
            {
                var parser = new DocxExamParser();
                var examModel = parser.Parse(ms);
                parsed = new ParsedExamParserResult { Exam = examModel };
            }

            var result = new ImportResultDto
            {
                Success = parsed.Exam.Errors.Count == 0,
                Errors = parsed.Exam.Errors,
                QuestionCount = parsed.Exam.TotalQuestions,
                PassageCount = parsed.Exam.Parts.Sum(p => p.Passages.Count),
                ImageCount = parsed.Exam.Parts.SelectMany(p => p.Passages).SelectMany(p => p.Documents).Count(d => d.Type == "image")
            };

            // contract: nothing is written unless the file is 100% valid
            if (!result.Success) return result;

            var title = !string.IsNullOrWhiteSpace(input.Title)
                ? input.Title
                : (string.IsNullOrWhiteSpace(parsed.Exam.Title) ? input.FileName : parsed.Exam.Title);

            var exam = new Exam
            {
                Title = title,
                Description = parsed.Exam.TestName,
                DurationMinutes = input.DurationMinutes <= 0 ? 75 : input.DurationMinutes,
                TotalQuestions = parsed.Exam.TotalQuestions,
                Status = ExamStatus.Draft,
                SourceFile = input.FileName,
                ShuffleQuestions = true,
                ShuffleOptions = true,
                OpenAt = DateTime.Now,
                CloseAt = DateTime.Now.AddDays(7)
            };

            var examId = await _examRepo.InsertAndGetIdAsync(exam);

            foreach (var part in parsed.Exam.Parts.OrderBy(p => p.Number))
            {
                foreach (var p in part.Passages.OrderBy(x => x.OrderIndex))
                {
                    var passage = new Passage
                    {
                        ExamId = examId,
                        GroupCode = p.Id,
                        Part = p.Part,
                        PassageType = p.Type,
                        Header = p.Header,
                        OrderIndex = p.OrderIndex
                    };
                    var passageId = await _passageRepo.InsertAndGetIdAsync(passage);

                    var order = 0;
                    string buffer = null;
                    foreach (var d in p.Documents)
                    {
                        if (d.Type == "text")
                        {
                            buffer = buffer == null ? d.Text : buffer + "\n\n" + d.Text;
                            continue;
                        }
                        if (buffer != null)
                        {
                            await _documentRepo.InsertAsync(new PassageDocument
                            {
                                PassageId = passageId, DocumentType = "text", Text = buffer, OrderIndex = order++
                            });
                            buffer = null;
                        }
                        await _documentRepo.InsertAsync(new PassageDocument
                        {
                            PassageId = passageId,
                            DocumentType = d.Type,
                            Label = d.Label,
                            ImagePath = SaveImageAsync(examId, passage.Id, order, d),
                            OrderIndex = order++
                        });
                    }
                    if (buffer != null)
                    {
                        await _documentRepo.InsertAsync(new PassageDocument
                        {
                            PassageId = passageId, DocumentType = "text", Text = buffer, OrderIndex = order
                        });
                    }

                    foreach (var q in p.Questions)
                        await InsertQuestionAsync(examId, passageId, q);
                }

                foreach (var q in part.StandaloneQuestions)
                    await InsertQuestionAsync(examId, null, q);
            }

            await CurrentUnitOfWork.SaveChangesAsync();
            result.ExamId = examId;
            return result;
        }

        private string SaveImageAsync(long examId, long passageId, int order, DocxExamParser.ParsedDocument d)
        {
            if (d.ImageBytes == null || d.ImageBytes.Length == 0)
                return d.ImageSrc; // ảnh do giáo viên đặt sẵn trong wwwroot/images

            var dir = Path.Combine(_uploadsRoot, "exams", examId.ToString());
            Directory.CreateDirectory(dir);
            var fileName = $"p{passageId}_{order}{d.ImageExtension ?? ".png"}";
            File.WriteAllBytes(Path.Combine(dir, fileName), d.ImageBytes);
            return $"uploads/exams/{examId}/{fileName}";
        }

        private async Task InsertQuestionAsync(long examId, long? passageId, DocxExamParser.ParsedQuestion q)
        {
            var question = new Question
            {
                ExamId = examId,
                PassageId = passageId,
                Number = q.Number,
                Part = q.Part,
                Stem = q.Stem,
                AnswerKey = q.AnswerKey,
                Hint = q.Hint,
                OrderIndex = q.OrderIndex,
                ShuffleQuestions = passageId == null ? "yes" : "group"
            };
            var questionId = await _questionRepo.InsertAndGetIdAsync(question);
            var i = 0;
            foreach (var key in q.Options.Keys.OrderBy(k => k))
            {
                await _optionRepo.InsertAsync(new QuestionOption
                {
                    QuestionId = questionId,
                    OptionKey = key,
                    Text = q.Options[key],
                    OrderIndex = i++
                });
            }
        }

        // ---------------------------------------------------------------- publish / edit
        [AbpAuthorize(PermissionNames.Pages_Exam)]
        public async Task PublishAsync(ExamIdInput input)
        {
            var exam = await _examRepo.GetAsync(input.Id);
            if (exam.TotalQuestions <= 0)
                throw new UserFriendlyException("Đề chưa có câu hỏi nào, không thể xuất bản.");
            exam.Status = ExamStatus.Published;
            exam.PublishedAt = DateTime.Now;
            await _examRepo.UpdateAsync(exam);
        }

        [AbpAuthorize(PermissionNames.Pages_Exam)]
        public async Task UnpublishAsync(ExamIdInput input)
        {
            var exam = await _examRepo.GetAsync(input.Id);
            exam.Status = ExamStatus.PendingPublish;
            await _examRepo.UpdateAsync(exam);
        }

        [AbpAuthorize(PermissionNames.Pages_Exam)]
        public async Task DeleteAsync(ExamIdInput input)
        {
            var id = input.Id;
            var questions = (await _questionRepo.GetAllListAsync()).Where(q => q.ExamId == id).ToList();
            foreach (var q in questions)
            {
                foreach (var o in (await _optionRepo.GetAllListAsync()).Where(o => o.QuestionId == q.Id).ToList())
                    await _optionRepo.DeleteAsync(o);
                await _questionRepo.DeleteAsync(q);
            }
            var passages = (await _passageRepo.GetAllListAsync()).Where(p => p.ExamId == id).ToList();
            foreach (var p in passages)
            {
                foreach (var d in (await _documentRepo.GetAllListAsync()).Where(d => d.PassageId == p.Id).ToList())
                    await _documentRepo.DeleteAsync(d);
                await _passageRepo.DeleteAsync(p);
            }
            await _examRepo.DeleteAsync(id);
        }

        [AbpAuthorize(PermissionNames.Pages_Exam)]
        public async Task UpdateQuestionAsync(UpdateQuestionInput input)
        {
            var q = await _questionRepo.GetAsync(input.QuestionId);
            q.Stem = input.Stem;
            q.Hint = input.Hint;
            if (!string.IsNullOrWhiteSpace(input.AnswerKey)) q.AnswerKey = input.AnswerKey.ToUpperInvariant();
            await _questionRepo.UpdateAsync(q);

            foreach (var opt in input.Options)
            {
                var existing = (await _optionRepo.GetAllListAsync())
                    .FirstOrDefault(o => o.QuestionId == q.Id && o.OptionKey == opt.Key.ToUpperInvariant());
                if (existing != null)
                {
                    existing.Text = opt.Text;
                    await _optionRepo.UpdateAsync(existing);
                }
                else
                {
                    await _optionRepo.InsertAsync(new QuestionOption
                    {
                        QuestionId = q.Id, OptionKey = opt.Key.ToUpperInvariant(), Text = opt.Text, OrderIndex = opt.OrderIndex
                    });
                }
            }
        }

        // ---------------------------------------------------------------- statistics
        [AbpAuthorize(PermissionNames.Pages_Exam)]
        public async Task<ExamStatsDto> GetStatsAsync(long id)
        {
            var exam = await _examRepo.GetAsync(id);
            var attempts = (await _attemptRepo.GetAllListAsync()).Where(a => a.ExamId == id).ToList();
            var attemptIds = attempts.Select(a => a.Id).ToList();
            var answers = (await _answerRepo.GetAllListAsync()).Where(a => attemptIds.Contains(a.AttemptId)).ToList();
            var questions = (await _questionRepo.GetAllListAsync()).Where(q => q.ExamId == id).ToList();

            var stats = questions.Select(q =>
            {
                var qa = answers.Where(a => a.QuestionId == q.Id).ToList();
                var wrong = qa.Count(a => !a.IsCorrect);
                var top = qa.Where(a => !a.IsCorrect && a.SelectedKey != null)
                    .GroupBy(a => a.SelectedKey).OrderByDescending(g => g.Count()).FirstOrDefault();
                return new QuestionStatDto
                {
                    Number = q.Number,
                    Part = q.Part,
                    AnswerKey = q.AnswerKey,
                    TotalAnswers = qa.Count,
                    WrongAnswers = wrong,
                    WrongRate = qa.Count == 0 ? 0 : Math.Round(wrong * 100.0 / qa.Count, 0),
                    MostChosenWrongKey = top?.Key
                };
            }).OrderByDescending(s => s.WrongRate).ThenByDescending(s => s.WrongAnswers).Take(10).ToList();

            return new ExamStatsDto
            {
                ExamId = exam.Id,
                Title = exam.Title,
                AttemptCount = attempts.Count,
                SubmittedCount = attempts.Count(a => a.Status != AttemptStatus.InProgress),
                AverageScore = attempts.Any() ? Math.Round(attempts.Average(a => a.Score), 1) : 0,
                HighestScore = attempts.Any() ? attempts.Max(a => a.Score) : 0,
                AverageDurationMinutes = attempts.Any() ? Math.Round(attempts.Average(a => a.DurationSeconds) / 60.0, 1) : 0,
                TopWrongQuestions = stats
            };
        }

        public async Task<List<StudentExamDto>> GetStudentExamsAsync(StudentExamsInput input)
        {
            var exams = (await _examRepo.GetAllListAsync())
                .Where(e => e.Status == ExamStatus.Published)
                .OrderByDescending(e => e.PublishedAt)
                .ToList();
            var attempts = await _attemptRepo.GetAllListAsync();
            var userId = AbpSession.UserId ?? 0;
            var code = input?.StudentCode;

            return exams.Select(e =>
            {
                var all = attempts
                    .Where(a => a.ExamId == e.Id && (code != null ? a.StudentCode == code : a.UserId == userId))
                    .ToList();
                var done = all.Where(a => a.Status != AttemptStatus.InProgress)
                    .OrderByDescending(a => a.SubmittedAt).ToList();
                var mine = done.FirstOrDefault();
                var running = all.FirstOrDefault(a => a.Status == AttemptStatus.InProgress);

                return new StudentExamDto
                {
                    Id = e.Id,
                    Title = e.Title,
                    DurationMinutes = e.DurationMinutes,
                    TotalQuestions = e.TotalQuestions,
                    CloseAt = e.CloseAt,
                    AlreadyAttempted = mine != null,
                    LastAttemptId = mine?.Id,
                    LastScore = mine?.Score,
                    AttemptCount = done.Count,
                    BestScore = done.Count == 0 ? (int?)null : done.Max(a => a.Score),
                    LastSubmittedAt = mine?.SubmittedAt,
                    InProgressAttemptId = running?.Id
                };
            }).ToList();
        }

        /// <summary>Lịch sử các lượt làm bài của một sinh viên (để làm lại / so sánh điểm).</summary>
        public async Task<List<StudentAttemptDto>> GetStudentAttemptsAsync(StudentAttemptsInput input)
        {
            var exam = await _examRepo.GetAsync(input.ExamId);
            var userId = AbpSession.UserId ?? 0;
            var all = (await _attemptRepo.GetAllListAsync())
                .Where(a => a.ExamId == exam.Id &&
                            (input.StudentCode != null ? a.StudentCode == input.StudentCode : a.UserId == userId))
                .OrderByDescending(a => a.StartedAt)
                .Select(a => new StudentAttemptDto
                {
                    Id = a.Id,
                    ExamId = a.ExamId,
                    ExamTitle = exam.Title,
                    Score = a.Score,
                    CorrectCount = a.CorrectCount,
                    WrongCount = a.WrongCount,
                    SkippedCount = a.SkippedCount,
                    StartedAt = a.StartedAt,
                    SubmittedAt = a.SubmittedAt,
                    DurationSeconds = a.DurationSeconds,
                    Status = a.Status == AttemptStatus.InProgress ? "InProgress"
                           : a.Status == AttemptStatus.AutoSubmitted ? "AutoSubmitted" : "Submitted"
                })
                .ToList();
            return all;
        }
    }

    internal class ParsedExamParserResult
    {
        public DocxExamParser.ParsedExam Exam { get; set; }
    }
}
