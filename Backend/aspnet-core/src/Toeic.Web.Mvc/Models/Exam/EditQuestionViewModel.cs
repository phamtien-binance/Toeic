using System.Collections.Generic;
using Toeic.ReadingTest.Dto;

namespace Toeic.Web.Models.Exam
{
    public class EditQuestionViewModel
    {
        public long ExamId { get; set; }
        public string ExamTitle { get; set; }
        public long QuestionId { get; set; }
        public int Number { get; set; }
        public int Part { get; set; }
        public string Stem { get; set; }
        public string Hint { get; set; }
        public string AnswerKey { get; set; }
        public List<OptionDto> Options { get; set; } = new List<OptionDto>();
        public string PreviousStem { get; set; }
        public string PreviousHint { get; set; }
        public string PreviousAnswerKey { get; set; }
    }
}
