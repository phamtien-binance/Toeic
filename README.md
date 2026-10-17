# TOEIC Reading — Đề giữa kỳ (HUTECH · 23DTHD5 · 2380614025)

Ứng dụng thi TOEIC Reading: **giảng viên dùng web**, **sinh viên dùng app mobile**, backend ASP.NET Boilerplate + SQL Server.

```
kiemtraa/
├─ Backend/aspnet-core/                 # Solution ABP (đổi tên từ template 2380600189 → Toeic)
│  └─ src/
│     ├─ Toeic.Core/ReadingTest/        # Entity: Exam, Passage, PassageDocument, Question, QuestionOption, Attempt, AttemptAnswer
│     ├─ Toeic.Application/ReadingTest/ # ExamAppService, AttemptAppService, DocxExamParser (bộ thẻ HERMES-EXAM-TAG)
│     ├─ Toeic.EntityFrameworkCore/     # DbContext + migration ReadingTest_Tables
│     ├─ Toeic.Web.Host/                # REST API cho app mobile (http://localhost:5000, https://localhost:44311)
│     └─ Toeic.Web.Mvc/                 # Web giảng viên (http://localhost:5001/Exam), có đăng nhập
├─ Frontend/
│  ├─ design/                           # Prototype HTML trắng–xanh (teacher + mobile)
│  ├─ figma-kit/                        # Bản standalone + script import Figma + SVG đã render
│  └─ student_app/                      # Flutter app cho sinh viên
└─ Files/
   ├─ TOEIC_Reading_Test3_Tagged.docx   # Đề thi có bộ thẻ (deliverable 1)
   ├─ BaoCao_TOEIC_Reading.docx/.pdf    # Báo cáo đồ án (deliverable 2)
   ├─ KichBan_Demo.md                   # Kịch bản quay video demo từng chức năng
   ├─ images/                           # Ảnh cắt từ đề gốc
   └─ screenshots/                      # Ảnh chụp màn hình dùng trong báo cáo
```

## 0. Tài khoản & quyền

| Vai trò | Đăng nhập | Phạm vi |
|---|---|---|
| Giảng viên | `admin` / `123qwe` tại http://localhost:5001/Account/Login | Toàn bộ trang quản trị (quyền `Pages.Exam`); API quản trị trả 401 nếu chưa đăng nhập |
| Sinh viên | Mã số sinh viên trên app Flutter (không cần mật khẩu) | Chỉ dùng các API sinh viên: `GetStudentExams`, `Attempt/Start`, `Attempt/SaveAnswers`, `Attempt/Submit`, `Attempt/GetResult` |


## 1. Chạy backend

```bash
# Tạo CSDL (lần đầu)
cd Backend/aspnet-core/src/Toeic.Migrator
echo Y | dotnet run

# API + Swagger
cd ../Toeic.Web.Host
dotnet run          # http://localhost:5000  |  https://localhost:44311

# Web giảng viên
cd ../Toeic.Web.Mvc
dotnet run --urls http://localhost:5001   # mở http://localhost:5001/Exam
```

Chuỗi kết nối (trong `appsettings.json`):

```
Server=localhost; Database=ToeicDb; Trusted_Connection=True; TrustServerCertificate=True;
```

## 2. Chạy app sinh viên

```bash
cd Frontend/student_app
flutter pub get
flutter run -d chrome --dart-define=API_BASE=http://localhost:5000
# hoặc build web:
flutter build web --release   # build/web, serve bằng: python -m http.server 8080
```

Android emulator dùng mặc định `http://10.0.2.2:5000` (không cần dart-define).

## 3. Import đề thi

1. Mở `http://localhost:5001/Exam/Import`
2. Chọn file `TOEIC_Reading_Test3_Tagged.docx`
3. File sai tag/thiếu đáp án → danh sách lỗi hiện ngay, **không ghi CSDL**
4. File hợp lệ → chuyển sang trang xem trước → bấm **Xuất bản** để sinh viên thấy đề trên app

## 3.1. Ảnh trong file Word

Khi import, ảnh nhúng trong `.docx` được trích ra và lưu vào thư mục cấu hình `Uploads:Root`
(mặc định `Backend/uploads`), đường dẫn ghi vào `RtPassageDocuments.ImagePath` dạng
`uploads/exams/{examId}/{file}.png`. Cả web (5001) và API (5000) phục vụ thư mục này qua `/uploads`,
nên đề có biểu mẫu/hoá đơn/tin nhắn vẫn hiển thị đúng trên web giảng viên và app sinh viên.

## 4. API chính (ABP dynamic Web API)

| Method | Endpoint | Việc |
|---|---|---|
| GET | `/api/services/app/Exam/GetAll` | Danh sách đề (giảng viên) |
| GET | `/api/services/app/Exam/GetDetail?id=1` | Chi tiết đề + bài đọc + câu hỏi |
| POST | `/api/services/app/Exam/ImportFromDocx` | Import file Word (base64) |
| POST | `/api/services/app/Exam/Publish` | Xuất bản đề (`{ "id": 1 }`) |
| POST | `/api/services/app/Exam/UpdateQuestion` | Sửa câu hỏi (nội dung, 4 phương án, đáp án, gợi ý) — web dùng form `/Exam/EditQuestion` |
| GET | `/api/services/app/Exam/GetStudentExams?studentCode=...` | Đề đã xuất bản cho sinh viên |
| POST | `/api/services/app/Attempt/Start` | Bắt đầu / tiếp tục bài thi |
| POST | `/api/services/app/Attempt/SaveAnswers` | Lưu tạm đáp án |
| POST | `/api/services/app/Attempt/Submit` | Nộp bài + chấm điểm |
| GET | `/api/services/app/Attempt/GetResult?attemptId=1` | Kết quả + đáp án + gợi ý |
| GET | `/api/services/app/Exam/GetStats?id=1` | Thống kê, top 10 câu sai |

## 5. Bộ thẻ trong file Word (HERMES-EXAM-TAG 1.0)

`[[DOC]] [[META]] [[PART]] [[GROUP]] [[PASSAGE]] [[IMAGE]] [[Q]] [[OPT]] [[ANS]] [[HINT]]`

- `ANS key="C"` lưu **key** chứ không lưu vị trí → hoán vị phương án vẫn chấm đúng
- `shuffle_questions`: `yes` (Part 5), `group` (Part 6/7, giữ bài đọc đi kèm), `no`
- Khối `[[SPEC ignore="true"]]` và `[[ANSWER_SHEET ignore="true"]]` bị parser bỏ qua

## 6. Ghi chú kỹ thuật

- Template ABP đặt tên project `2380600189` (bắt đầu bằng số) nên **không compile** — đã đổi toàn bộ namespace/assembly sang `Toeic`.
- FK dùng `DeleteBehavior.Restrict` ở các quan hệ nhiều nhánh (Exam→Passage, Exam→Question, Attempt→Exam) để tránh lỗi SQL Server 1785 "multiple cascade paths".
- Các endpoint POST nhận **DTO** (`ExamIdInput`, `StartAttemptInput`…), không nhận tham số nguyên thuỷ, vì dynamic Web API bind primitive qua query string.
- Phân quyền chi tiết (giảng viên/sinh viên) đang để mở cho mục đích demo; bật lại bằng `[AbpAuthorize]` + permission `Pages.Exam` khi triển khai thật.

## 7. Link nộp bài

- Figma (12 frame, 2 page): https://www.figma.com/design/Yn5o5kAnlpF0TqFaq1dr6k/Untitled
- Báo cáo: `Files/BaoCao_TOEIC_Reading.docx` (kèm bản PDF)
- Kịch bản quay demo: `Files/KichBan_Demo.md`
- Repository: `git log` trong thư mục này (commit đầu tiên chứa toàn bộ source + files)
