using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;

namespace Toeic.ReadingTest
{
    public enum ExamStatus
    {
        Draft = 0,
        PendingPublish = 1,
        Published = 2,
        Archived = 3
    }

    public enum AttemptStatus
    {
        InProgress = 0,
        Submitted = 1,
        AutoSubmitted = 2
    }

    [Table("RtExams")]
    public class Exam : FullAuditedEntity<long>
    {
        [Required, MaxLength(256)]
        public string Title { get; set; }

        [MaxLength(512)]
        public string Description { get; set; }

        public int DurationMinutes { get; set; }

        public int TotalQuestions { get; set; }

        public ExamStatus Status { get; set; }

        [MaxLength(256)]
        public string SourceFile { get; set; }

        public DateTime? PublishedAt { get; set; }

        public DateTime? OpenAt { get; set; }

        public DateTime? CloseAt { get; set; }

        public bool ShuffleQuestions { get; set; }

        public bool ShuffleOptions { get; set; }

        public virtual ICollection<Passage> Passages { get; set; } = new List<Passage>();

        public virtual ICollection<Question> Questions { get; set; } = new List<Question>();
    }

    [Table("RtPassages")]
    public class Passage : FullAuditedEntity<long>
    {
        public long ExamId { get; set; }

        [ForeignKey(nameof(ExamId))]
        public virtual Exam Exam { get; set; }

        /// <summary>Group code from the Word file, e.g. "P7-12".</summary>
        [MaxLength(64)]
        public string GroupCode { get; set; }

        public int Part { get; set; }

        /// <summary>article | e-mail | form | notice | survey | invoice | estimate | advertisement | press-release | text-message | manual | letter</summary>
        [MaxLength(64)]
        public string PassageType { get; set; }

        [MaxLength(512)]
        public string Header { get; set; }

        public int OrderIndex { get; set; }

        public virtual ICollection<PassageDocument> Documents { get; set; } = new List<PassageDocument>();
    }

    [Table("RtPassageDocuments")]
    public class PassageDocument : FullAuditedEntity<long>
    {
        public long PassageId { get; set; }

        [ForeignKey(nameof(PassageId))]
        public virtual Passage Passage { get; set; }

        [MaxLength(256)]
        public string Label { get; set; }

        [MaxLength(64)]
        public string DocumentType { get; set; }

        public string Text { get; set; }

        /// <summary>Relative path of the cropped image inside wwwroot/uploads.</summary>
        [MaxLength(512)]
        public string ImagePath { get; set; }

        public int OrderIndex { get; set; }
    }

    [Table("RtQuestions")]
    public class Question : FullAuditedEntity<long>
    {
        public long ExamId { get; set; }

        [ForeignKey(nameof(ExamId))]
        public virtual Exam Exam { get; set; }

        public long? PassageId { get; set; }

        [ForeignKey(nameof(PassageId))]
        public virtual Passage Passage { get; set; }

        /// <summary>Original question number in the test (101-200).</summary>
        public int Number { get; set; }

        public int Part { get; set; }

        public string Stem { get; set; }

        /// <summary>Correct option key (A-D). Never the display position.</summary>
        [MaxLength(4)]
        public string AnswerKey { get; set; }

        public string Hint { get; set; }

        public int OrderIndex { get; set; }

        [MaxLength(16)]
        public string ShuffleQuestions { get; set; }

        public virtual ICollection<QuestionOption> Options { get; set; } = new List<QuestionOption>();
    }

    [Table("RtQuestionOptions")]
    public class QuestionOption : FullAuditedEntity<long>
    {
        public long QuestionId { get; set; }

        [ForeignKey(nameof(QuestionId))]
        public virtual Question Question { get; set; }

        [Required, MaxLength(4)]
        public string OptionKey { get; set; }

        public string Text { get; set; }

        public int OrderIndex { get; set; }
    }

    [Table("RtAttempts")]
    public class Attempt : FullAuditedEntity<long>
    {
        public long ExamId { get; set; }

        [ForeignKey(nameof(ExamId))]
        public virtual Exam Exam { get; set; }

        public long UserId { get; set; }

        [MaxLength(64)]
        public string StudentCode { get; set; }

        [MaxLength(128)]
        public string StudentName { get; set; }

        public DateTime StartedAt { get; set; }

        public DateTime? SubmittedAt { get; set; }

        public AttemptStatus Status { get; set; }

        public int Score { get; set; }

        public int CorrectCount { get; set; }

        public int WrongCount { get; set; }

        public int SkippedCount { get; set; }

        public int DurationSeconds { get; set; }

        public virtual ICollection<AttemptAnswer> Answers { get; set; } = new List<AttemptAnswer>();
    }

    [Table("RtAttemptAnswers")]
    public class AttemptAnswer : FullAuditedEntity<long>
    {
        public long AttemptId { get; set; }

        [ForeignKey(nameof(AttemptId))]
        public virtual Attempt Attempt { get; set; }

        public long QuestionId { get; set; }

        [ForeignKey(nameof(QuestionId))]
        public virtual Question Question { get; set; }

        [MaxLength(4)]
        public string SelectedKey { get; set; }

        public bool IsCorrect { get; set; }

        public DateTime AnsweredAt { get; set; }
    }
}
