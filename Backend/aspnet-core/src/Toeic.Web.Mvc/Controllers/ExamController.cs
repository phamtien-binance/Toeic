using System;
using System.IO;
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

        public async Task<IActionResult> Stats(long id)
        {
            ViewBag.Title = "Thống kê và phân tích";
            ViewBag.Active = "stats";
            var stats = await _examAppService.GetStatsAsync(id);
            ViewBag.Subtitle = $"{stats.AttemptCount} lượt làm · điểm TB {stats.AverageScore}";
            return View(stats);
        }
    }
}
