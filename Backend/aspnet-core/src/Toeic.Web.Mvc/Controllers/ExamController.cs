using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Abp.AspNetCore.Mvc.Authorization;
using Toeic.Authorization;
using Toeic.Controllers;
using Toeic.ReadingTest;
using Toeic.ReadingTest.Dto;

namespace Toeic.Web.Controllers
{
    [AbpMvcAuthorize(PermissionNames.Pages_Exam)]
    public class ExamController : ToeicControllerBase
    {
        private readonly IExamAppService _examAppService;
        private readonly IAttemptAppService _attemptAppService;

        public ExamController(IExamAppService examAppService, IAttemptAppService attemptAppService)
        {
            _examAppService = examAppService;
            _attemptAppService = attemptAppService;
        }

        public async Task<IActionResult> Index()
        {
            ViewBag.Title = "Đề thi";
            ViewBag.Active = "exams";
            var exams = await _examAppService.GetAllAsync(new GetExamsInput());
            ViewBag.Subtitle = $"{exams.Count} đề · {exams.FindAll(e => e.Status == "Published").Count} đã xuất bản";
            return View(exams);
        }

        [HttpGet]
        public IActionResult Import()
        {
            ViewBag.Title = "Import đề thi từ Word";
            ViewBag.Active = "import";
            ViewBag.Subtitle = "Chỉ nhận file .docx có bộ thẻ HERMES-EXAM-TAG 1.0";
            return View(new ImportExamInput { DurationMinutes = 75 });
        }

        [HttpPost]
        [RequestSizeLimit(50_000_000)]
        public async Task<IActionResult> Import(ImportExamInput input, IFormFile file)
        {
            ViewBag.Title = "Import đề thi từ Word";
            ViewBag.Active = "import";
            ViewBag.Subtitle = "Chỉ nhận file .docx có bộ thẻ HERMES-EXAM-TAG 1.0";

            if (file == null || file.Length == 0)
            {
                ViewBag.Error = "Chưa chọn file .docx.";
                return View(input);
            }
            if (!Path.GetExtension(file.FileName).Equals(".docx", StringComparison.OrdinalIgnoreCase))
            {
                ViewBag.Error = "Chỉ hỗ trợ file .docx.";
                return View(input);
            }

            using var ms = new MemoryStream();
            await file.CopyToAsync(ms);
            input.FileName = file.FileName;
            input.ContentBase64 = Convert.ToBase64String(ms.ToArray());

            var result = await _examAppService.ImportFromDocxAsync(input);
            ViewBag.Result = result;
            ViewBag.FileName = file.FileName;
            ViewBag.FileSize = file.Length;
            if (result.Success)
            {
                TempData["Flash"] = $"Đã import {result.QuestionCount} câu hỏi, {result.PassageCount} bài đọc.";
                return RedirectToAction("Preview", new { id = result.ExamId });
            }
            return View(input);
        }

        public async Task<IActionResult> Preview(long id)
        {
            ViewBag.Title = "Xem trước và sửa đề";
            ViewBag.Active = "exams";
            var exam = await _examAppService.GetDetailAsync(id);
            ViewBag.Subtitle = $"{exam.TotalQuestions} câu · {exam.DurationMinutes} phút · {exam.Passages.Count} bài đọc";
            return View(exam);
        }

        [HttpPost]
        public async Task<IActionResult> Publish(long id)
        {
            await _examAppService.PublishAsync(new ExamIdInput { Id = id });
            TempData["Flash"] = "Đề đã được xuất bản. Sinh viên có thể bắt đầu làm bài trên app.";
            return RedirectToAction("Preview", new { id });
        }

        [HttpPost]
        public async Task<IActionResult> Unpublish(long id)
        {
            await _examAppService.UnpublishAsync(new ExamIdInput { Id = id });
            TempData["Flash"] = "Đã thu hồi đề.";
            return RedirectToAction("Preview", new { id });
        }

        [HttpPost]
        public async Task<IActionResult> Delete(long id)
        {
            await _examAppService.DeleteAsync(new ExamIdInput { Id = id });
            TempData["Flash"] = "Đã xoá đề thi.";
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> EditQuestion(long id, long questionId)
        {
            var exam = await _examAppService.GetDetailAsync(id);
            QuestionDto question = null;
            foreach (var p in exam.Passages)
            {
                question = p.Questions.Find(q => q.Id == questionId);
                if (question != null) break;
            }
            question ??= exam.StandaloneQuestions.Find(q => q.Id == questionId);
            if (question == null)
            {
                TempData["Flash"] = "Không tìm thấy câu hỏi.";
                return RedirectToAction("Preview", new { id });
            }

            ViewBag.Title = $"Sửa câu {question.Number}";
            ViewBag.Active = "exams";
            ViewBag.Subtitle = exam.Title;

            return View("EditQuestion", new Toeic.Web.Models.Exam.EditQuestionViewModel
            {
                ExamId = exam.Id,
                ExamTitle = exam.Title,
                QuestionId = question.Id,
                Number = question.Number,
                Part = question.Part,
                Stem = question.Stem,
                Hint = question.Hint,
                AnswerKey = question.AnswerKey,
                Options = question.Options,
                PreviousStem = question.Stem,
                PreviousHint = question.Hint,
                PreviousAnswerKey = question.AnswerKey
            });
        }

        [HttpPost]
        public async Task<IActionResult> EditQuestion(
            long id, long questionId, string stem, string hint, string answerKey,
            string optA, string optB, string optC, string optD)
        {
            var texts = new[] { optA, optB, optC, optD };
            if (texts.Any(string.IsNullOrWhiteSpace))
            {
                TempData["Flash"] = "Không lưu: phải có đủ nội dung 4 phương án.";
                return RedirectToAction("EditQuestion", new { id, questionId });
            }
            if (string.IsNullOrWhiteSpace(stem) || string.IsNullOrWhiteSpace(hint))
            {
                TempData["Flash"] = "Không lưu: nội dung câu hỏi và gợi ý không được để trống.";
                return RedirectToAction("EditQuestion", new { id, questionId });
            }

            await _examAppService.UpdateQuestionAsync(new UpdateQuestionInput
            {
                QuestionId = questionId,
                Stem = stem,
                Hint = hint,
                AnswerKey = answerKey,
                Options = new System.Collections.Generic.List<OptionDto>
                {
                    new OptionDto { Key = "A", Text = optA, OrderIndex = 0 },
                    new OptionDto { Key = "B", Text = optB, OrderIndex = 1 },
                    new OptionDto { Key = "C", Text = optC, OrderIndex = 2 },
                    new OptionDto { Key = "D", Text = optD, OrderIndex = 3 }
                }
            });
            TempData["Flash"] = "Đã lưu thay đổi câu hỏi.";
            return RedirectToAction("Preview", new { id });
        }

        public async Task<IActionResult> Stats(long id = 0)
        {
            // Vào từ menu trái (không có id) hoặc id không hợp lệ -> hiện danh sách để chọn đề
            if (id <= 0)
            {
                var exams = await _examAppService.GetAllAsync(new GetExamsInput());
                ViewBag.Title = "Thống kê và phân tích";
                ViewBag.Active = "stats";
                ViewBag.Subtitle = $"{exams.Count} đề — chọn đề để xem thống kê";
                return View("StatsChooser", exams);
            }

            ViewBag.Title = "Thống kê và phân tích";
            ViewBag.Active = "stats";
            ExamStatsDto stats;
            try
            {
                stats = await _examAppService.GetStatsAsync(id);
            }
            catch (Abp.Domain.Entities.EntityNotFoundException)
            {
                TempData["Flash"] = "Không tìm thấy đề cần xem thống kê.";
                return RedirectToAction("Stats");
            }
            ViewBag.Subtitle = $"{stats.AttemptCount} lượt làm · điểm TB {stats.AverageScore}";
            return View(stats);
        }
    }
}
