using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Domain.Repositories;
using Abp.UI;
using Toeic.ReadingTest.Dto;

namespace Toeic.ReadingTest
{
    public interface IAttemptAppService : IApplicationService
    {
        Task<AttemptDetailDto> StartAsync(StartAttemptInput input);
        Task SaveAnswersAsync(SaveAnswersInput input);
        Task<AttemptResultDto> SubmitAsync(SubmitAttemptInput input);
        Task<AttemptResultDto> GetResultAsync(long attemptId);
        Task<AttemptDetailDto> GetAsync(long attemptId);
    }

    public class AttemptAppService : ToeicAppServiceBase, IAttemptAppService
    {
        private readonly IRepository<Exam, long> _examRepo;
        private readonly IRepository<Passage, long> _passageRepo;
        private readonly IRepository<PassageDocument, long> _documentRepo;
        private readonly IRepository<Question, long> _questionRepo;
        private readonly IRepository<QuestionOption, long> _optionRepo;
        private readonly IRepository<Attempt, long> _attemptRepo;
        private readonly IRepository<AttemptAnswer, long> _answerRepo;

        public AttemptAppService(
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
        }

        public async Task<AttemptDetailDto> StartAsync(StartAttemptInput input)
        {
            var exam = await _examRepo.GetAsync(input.ExamId);
            if (exam.Status != ExamStatus.Published)
                throw new UserFriendlyException("Đề thi chưa được xuất bản.");

            var userId = AbpSession.UserId ?? 0;
            var existing = (await _attemptRepo.GetAllListAsync())
                .FirstOrDefault(a => a.ExamId == exam.Id && a.UserId == userId
                                     && (a.StudentCode == input.StudentCode || input.StudentCode == null)
                                     && a.Status == AttemptStatus.InProgress);

            if (existing != null) return await BuildDetailAsync(existing);

            var attempt = new Attempt
            {
                ExamId = exam.Id,
                UserId = userId,
                StudentCode = input.StudentCode,
                StudentName = input.StudentName,
                StartedAt = DateTime.Now,
                Status = AttemptStatus.InProgress
            };
            attempt.Id = await _attemptRepo.InsertAndGetIdAsync(attempt);
            await CurrentUnitOfWork.SaveChangesAsync();
            return await BuildDetailAsync(attempt);
        }

        public async Task SaveAnswersAsync(SaveAnswersInput input)
        {
            var attempt = await _attemptRepo.GetAsync(input.AttemptId);
            if (attempt.Status != AttemptStatus.InProgress)
                throw new UserFriendlyException("Bài thi đã nộp, không thể sửa đáp án.");

            var existing = (await _answerRepo.GetAllListAsync()).Where(a => a.AttemptId == attempt.Id).ToList();
            foreach (var ans in input.Answers)
            {
                var row = existing.FirstOrDefault(a => a.QuestionId == ans.QuestionId);
                if (string.IsNullOrEmpty(ans.SelectedKey))
                {
                    if (row != null) await _answerRepo.DeleteAsync(row);
                    continue;
                }
                if (row == null)
                {
                    await _answerRepo.InsertAsync(new AttemptAnswer
                    {
                        AttemptId = attempt.Id,
                        QuestionId = ans.QuestionId,
                        SelectedKey = ans.SelectedKey.ToUpperInvariant(),
                        AnsweredAt = DateTime.Now
                    });
                }
                else
                {
                    row.SelectedKey = ans.SelectedKey.ToUpperInvariant();
                    row.AnsweredAt = DateTime.Now;
                    await _answerRepo.UpdateAsync(row);
                }
            }
            await CurrentUnitOfWork.SaveChangesAsync();
        }

        public async Task<AttemptResultDto> SubmitAsync(SubmitAttemptInput input)
        {
            var attempt = await _attemptRepo.GetAsync(input.AttemptId);
            if (attempt.Status != AttemptStatus.InProgress)
                return await GetResultAsync(input.AttemptId);

            var exam = await _examRepo.GetAsync(attempt.ExamId);
            var questions = (await _questionRepo.GetAllListAsync()).Where(q => q.ExamId == exam.Id).ToList();
            var answers = (await _answerRepo.GetAllListAsync()).Where(a => a.AttemptId == attempt.Id).ToList();

            var correct = 0; var wrong = 0; var skipped = 0;
            foreach (var q in questions)
            {
                var a = answers.FirstOrDefault(x => x.QuestionId == q.Id);
                if (a == null || string.IsNullOrEmpty(a.SelectedKey)) { skipped++; continue; }
                var ok = string.Equals(a.SelectedKey, q.AnswerKey, StringComparison.OrdinalIgnoreCase);
                a.IsCorrect = ok;
                await _answerRepo.UpdateAsync(a);
                if (ok) correct++; else wrong++;
            }

            attempt.CorrectCount = correct;
            attempt.WrongCount = wrong;
            attempt.SkippedCount = skipped;
            attempt.Score = questions.Count == 0 ? 0 : (int)Math.Round(correct * 100.0 / questions.Count);
            attempt.SubmittedAt = DateTime.Now;
            attempt.DurationSeconds = (int)(DateTime.Now - attempt.StartedAt).TotalSeconds;
            attempt.Status = input.AutoSubmitted ? AttemptStatus.AutoSubmitted : AttemptStatus.Submitted;
            await _attemptRepo.UpdateAsync(attempt);
            await CurrentUnitOfWork.SaveChangesAsync();

            return await GetResultAsync(attempt.Id);
        }

        public async Task<AttemptResultDto> GetResultAsync(long attemptId)
        {
            var attempt = await _attemptRepo.GetAsync(attemptId);
            var exam = await _examRepo.GetAsync(attempt.ExamId);
            var questions = (await _questionRepo.GetAllListAsync()).Where(q => q.ExamId == exam.Id).OrderBy(q => q.OrderIndex).ToList();
            var options = await _optionRepo.GetAllListAsync();
            var answers = (await _answerRepo.GetAllListAsync()).Where(a => a.AttemptId == attempt.Id).ToList();
            var passages = (await _passageRepo.GetAllListAsync()).Where(p => p.ExamId == exam.Id).ToList();
            var documents = (await _documentRepo.GetAllListAsync()).Where(d => passages.Any(p => p.Id == d.PassageId)).ToList();

            List<AttemptQuestionDto> Map(IEnumerable<Question> qs) => qs.Select(q =>
            {
                var a = answers.FirstOrDefault(x => x.QuestionId == q.Id);
                return new AttemptQuestionDto
                {
                    Id = q.Id,
                    Number = q.Number,
                    Part = q.Part,
                    PassageId = q.PassageId,
                    Stem = q.Stem,
                    AnswerKey = q.AnswerKey,
                    Hint = q.Hint,
                    SelectedKey = a?.SelectedKey,
                    IsCorrect = a == null ? (bool?)null : string.Equals(a.SelectedKey, q.AnswerKey, StringComparison.OrdinalIgnoreCase),
                    Options = options.Where(o => o.QuestionId == q.Id).OrderBy(o => o.OrderIndex)
                        .Select(o => new OptionDto { Key = o.OptionKey, Text = o.Text, OrderIndex = o.OrderIndex }).ToList()
                };
            }).ToList();

            return new AttemptResultDto
            {
                AttemptId = attempt.Id,
                ExamId = exam.Id,
                ExamTitle = exam.Title,
                Score = attempt.Score,
                TotalQuestions = questions.Count,
                CorrectCount = attempt.CorrectCount,
                WrongCount = attempt.WrongCount,
                SkippedCount = attempt.SkippedCount,
                DurationSeconds = attempt.DurationSeconds,
                Parts = new[] { 5, 6, 7 }.Where(p => questions.Any(q => q.Part == p)).Select(p =>
                {
                    var qs = questions.Where(q => q.Part == p).ToList();
                    return new PartScoreDto
                    {
                        Part = p,
                        Total = qs.Count,
                        Correct = qs.Count(q =>
                        {
                            var a = answers.FirstOrDefault(x => x.QuestionId == q.Id);
                            return a != null && string.Equals(a.SelectedKey, q.AnswerKey, StringComparison.OrdinalIgnoreCase);
                        })
                    };
                }).ToList(),
                Questions = Map(questions),
                Passages = passages.OrderBy(p => p.OrderIndex).Select(p => new PassageDto
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
                            Id = d.Id, Label = d.Label, DocumentType = d.DocumentType,
                            Text = d.Text, ImagePath = d.ImagePath, OrderIndex = d.OrderIndex
                        }).ToList()
                }).ToList()
            };
        }

        public async Task<AttemptDetailDto> GetAsync(long attemptId)
        {
            var attempt = await _attemptRepo.GetAsync(attemptId);
            return await BuildDetailAsync(attempt);
        }

        private async Task<AttemptDetailDto> BuildDetailAsync(Attempt attempt)
        {
            var exam = await _examRepo.GetAsync(attempt.ExamId);
            var questions = (await _questionRepo.GetAllListAsync()).Where(q => q.ExamId == exam.Id).OrderBy(q => q.OrderIndex).ToList();
            var options = await _optionRepo.GetAllListAsync();
            var answers = (await _answerRepo.GetAllListAsync()).Where(a => a.AttemptId == attempt.Id).ToList();
            var passages = (await _passageRepo.GetAllListAsync()).Where(p => p.ExamId == exam.Id).OrderBy(p => p.OrderIndex).ToList();
            var documents = (await _documentRepo.GetAllListAsync()).Where(d => passages.Any(p => p.Id == d.PassageId)).ToList();

            var elapsed = (int)(DateTime.Now - attempt.StartedAt).TotalSeconds;
            var remaining = Math.Max(0, exam.DurationMinutes * 60 - elapsed);

            List<AttemptQuestionDto> Map(IEnumerable<Question> qs, bool withAnswers) => qs.Select(q => new AttemptQuestionDto
            {
                Id = q.Id,
                Number = q.Number,
                Part = q.Part,
                PassageId = q.PassageId,
                Stem = q.Stem,
                AnswerKey = withAnswers ? q.AnswerKey : null,
                Hint = withAnswers ? q.Hint : null,
                SelectedKey = answers.FirstOrDefault(x => x.QuestionId == q.Id)?.SelectedKey,
                Options = options.Where(o => o.QuestionId == q.Id).OrderBy(o => o.OrderIndex)
                    .Select(o => new OptionDto { Key = o.OptionKey, Text = o.Text }).ToList()
            }).ToList();

            var dto = new AttemptDetailDto
            {
                Id = attempt.Id,
                ExamId = exam.Id,
                ExamTitle = exam.Title,
                DurationMinutes = exam.DurationMinutes,
                Status = attempt.Status.ToString(),
                StartedAt = attempt.StartedAt,
                SubmittedAt = attempt.SubmittedAt,
                RemainingSeconds = remaining,
                Questions = Map(questions, false),
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
                            Id = d.Id, Label = d.Label, DocumentType = d.DocumentType,
                            Text = d.Text, ImagePath = d.ImagePath, OrderIndex = d.OrderIndex
                        }).ToList()
                }).ToList()
            };
            return dto;
        }
    }
}
