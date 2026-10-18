using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Toeic.ReadingTest.Dto
{
    // ---------- shared ----------
    public class OptionDto
    {
        public string Key { get; set; }
        public string Text { get; set; }
        public int OrderIndex { get; set; }
    }

    public class QuestionDto
    {
        public long Id { get; set; }
        public int Number { get; set; }
        public int Part { get; set; }
        public long? PassageId { get; set; }
        public string Stem { get; set; }
        public string AnswerKey { get; set; }
        public string Hint { get; set; }
        public List<OptionDto> Options { get; set; } = new List<OptionDto>();
    }

    public class PassageDocumentDto
    {
        public long Id { get; set; }
        public string Label { get; set; }
        public string DocumentType { get; set; }
        public string Text { get; set; }
        public string ImagePath { get; set; }
        public int OrderIndex { get; set; }
    }

    public class PassageDto
    {
        public long Id { get; set; }
        public string GroupCode { get; set; }
        public int Part { get; set; }
        public string PassageType { get; set; }
        public string Header { get; set; }
        public int OrderIndex { get; set; }
        public List<PassageDocumentDto> Documents { get; set; } = new List<PassageDocumentDto>();
        public List<QuestionDto> Questions { get; set; } = new List<QuestionDto>();
    }

    // ---------- exams ----------
    public class ExamListDto
    {
        public long Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public int DurationMinutes { get; set; }
        public int TotalQuestions { get; set; }
        public string Status { get; set; }
        public string SourceFile { get; set; }
        public DateTime? PublishedAt { get; set; }
        public DateTime? OpenAt { get; set; }
        public DateTime? CloseAt { get; set; }
        public int AttemptCount { get; set; }
        public double AverageScore { get; set; }
        public DateTime CreationTime { get; set; }
    }

    public class ExamDetailDto
    {
        public long Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public int DurationMinutes { get; set; }
        public int TotalQuestions { get; set; }
        public string Status { get; set; }
        public string SourceFile { get; set; }
        public bool ShuffleQuestions { get; set; }
        public bool ShuffleOptions { get; set; }
        public List<PassageDto> Passages { get; set; } = new List<PassageDto>();
        public List<QuestionDto> StandaloneQuestions { get; set; } = new List<QuestionDto>();
    }

    public class ExamIdInput
    {
        public long Id { get; set; }
    }

    public class GetExamsInput
    {
        public string Keyword { get; set; }
        public string Status { get; set; }
    }

    public class ImportExamInput
    {
        [Required]
        public string FileName { get; set; }

        /// <summary>Base64 content of the .docx file.</summary>
        [Required]
        public string ContentBase64 { get; set; }

        public string Title { get; set; }
        public int DurationMinutes { get; set; } = 75;
    }

    public class ImportValidationError
    {
        public string Code { get; set; }
        public string Message { get; set; }
        public int? QuestionNumber { get; set; }
    }

    public class ImportResultDto
    {
        public bool Success { get; set; }
        public long? ExamId { get; set; }
        public int QuestionCount { get; set; }
        public int PassageCount { get; set; }
        public int ImageCount { get; set; }
        public List<ImportValidationError> Errors { get; set; } = new List<ImportValidationError>();
    }

    public class UpdateQuestionInput
    {
        public long QuestionId { get; set; }
        public string Stem { get; set; }
        public string Hint { get; set; }
        public string AnswerKey { get; set; }
        public List<OptionDto> Options { get; set; } = new List<OptionDto>();
    }

    // ---------- statistics ----------
    public class QuestionStatDto
    {
        public int Number { get; set; }
        public int Part { get; set; }
        public string AnswerKey { get; set; }
        public int TotalAnswers { get; set; }
        public int WrongAnswers { get; set; }
        public double WrongRate { get; set; }
        public string MostChosenWrongKey { get; set; }
    }

    public class ExamStatsDto
    {
        public long ExamId { get; set; }
        public string Title { get; set; }
        public int AttemptCount { get; set; }
        public int SubmittedCount { get; set; }
        public double AverageScore { get; set; }
        public int HighestScore { get; set; }
        public double AverageDurationMinutes { get; set; }
        public List<QuestionStatDto> TopWrongQuestions { get; set; } = new List<QuestionStatDto>();
    }

    // ---------- student / attempts ----------
    public class StartAttemptInput
    {
        public long ExamId { get; set; }
        public string StudentCode { get; set; }
        public string StudentName { get; set; }
    }

    public class StudentExamsInput
    {
        public string StudentCode { get; set; }
    }

    public class StudentExamDto
    {
        public long Id { get; set; }
        public string Title { get; set; }
        public int DurationMinutes { get; set; }
        public int TotalQuestions { get; set; }
        public DateTime? CloseAt { get; set; }
        public bool AlreadyAttempted { get; set; }
        public long? LastAttemptId { get; set; }
        public int? LastScore { get; set; }
        public int AttemptCount { get; set; }
        public int? BestScore { get; set; }
        public DateTime? LastSubmittedAt { get; set; }
        public long? InProgressAttemptId { get; set; }
    }

    public class StudentAttemptsInput
    {
        public long ExamId { get; set; }
        public string StudentCode { get; set; }
    }

    public class StudentAttemptDto
    {
        public long Id { get; set; }
        public long ExamId { get; set; }
        public string ExamTitle { get; set; }
        public int Score { get; set; }
        public int CorrectCount { get; set; }
        public int WrongCount { get; set; }
        public int SkippedCount { get; set; }
        public DateTime StartedAt { get; set; }
        public DateTime? SubmittedAt { get; set; }
        public int DurationSeconds { get; set; }
        public string Status { get; set; }
    }

    public class AttemptQuestionDto
    {
        public long Id { get; set; }
        public int Number { get; set; }
        public int Part { get; set; }
        public long? PassageId { get; set; }
        public string Stem { get; set; }
        public List<OptionDto> Options { get; set; } = new List<OptionDto>();
        public string SelectedKey { get; set; }
        public string AnswerKey { get; set; }
        public string Hint { get; set; }
        public bool? IsCorrect { get; set; }
    }

    public class AttemptDetailDto
    {
        public long Id { get; set; }
        public long ExamId { get; set; }
        public string ExamTitle { get; set; }
        public int DurationMinutes { get; set; }
        public string Status { get; set; }
        public DateTime StartedAt { get; set; }
        public DateTime? SubmittedAt { get; set; }
        public int RemainingSeconds { get; set; }
        public List<PassageDto> Passages { get; set; } = new List<PassageDto>();
        public List<AttemptQuestionDto> Questions { get; set; } = new List<AttemptQuestionDto>();
    }

    public class AnswerInput
    {
        public long QuestionId { get; set; }
        public string SelectedKey { get; set; }
    }

    public class SaveAnswersInput
    {
        public long AttemptId { get; set; }
        public List<AnswerInput> Answers { get; set; } = new List<AnswerInput>();
    }

    public class SubmitAttemptInput
    {
        public long AttemptId { get; set; }
        public bool AutoSubmitted { get; set; }
    }

    public class AttemptResultDto
    {
        public long AttemptId { get; set; }
        public long ExamId { get; set; }
        public string ExamTitle { get; set; }
        public int Score { get; set; }
        public int TotalQuestions { get; set; }
        public int CorrectCount { get; set; }
        public int WrongCount { get; set; }
        public int SkippedCount { get; set; }
        public int DurationSeconds { get; set; }
        public List<PartScoreDto> Parts { get; set; } = new List<PartScoreDto>();
        public List<PassageDto> Passages { get; set; } = new List<PassageDto>();
        public List<AttemptQuestionDto> Questions { get; set; } = new List<AttemptQuestionDto>();
    }

    public class PartScoreDto
    {
        public int Part { get; set; }
        public int Total { get; set; }
        public int Correct { get; set; }
    }
}
