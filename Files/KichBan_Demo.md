# Kịch bản quay demo — TOEIC Reading (giảng viên web + sinh viên mobile)

> Mục tiêu: quay full screen từng chức năng theo bảng tiêu chí chấm điểm, tổng ~8–10 phút.
> Quay 1920×1080, thu tiếng khi thao tác; hoặc quay không tiếng rồi lồng thuyết minh theo cột "Lời thoại".

## 0. Chuẩn bị trước khi quay (2 phút, không quay)

```bash
# 1. API sinh viên
cd Backend/aspnet-core/src/Toeic.Web.Host && dotnet run        # http://localhost:5000

# 2. Web giảng viên
cd ../Toeic.Web.Mvc && dotnet run --urls http://localhost:5001  # http://localhost:5001/Exam

# 3. App sinh viên (bản web để quay cho nét)
cd ../../../../Frontend/student_app
flutter run -d chrome --dart-define=API_BASE=http://localhost:5000
```

- Tài khoản giảng viên: **admin / 123qwe**
- Mã sinh viên dùng để quay: **2380614025** (đã có bài nộp 70 điểm) và **2380614888** (chưa làm bài)
- Mở sẵn 2 cửa sổ: Chrome trái = web giảng viên, Chrome phải = app sinh viên (chế độ điện thoại 390×844)
- Đã import sẵn đề `TOEIC_Reading_Test3_Tagged.docx` (đề id 2) và đang **Published**

## 1. Các cảnh quay

| # | Thời lượng | Thao tác trên màn hình | Lời thoại gợi ý |
|---|---|---|---|
| 1 | 0:00–0:40 | Mở `http://localhost:5001/Exam` → bị chuyển về trang đăng nhập → nhập admin/123qwe → vào danh sách đề | "Hệ thống tách hai phía: giảng viên dùng web có đăng nhập và phân quyền, sinh viên dùng app. Khu vực quản trị yêu cầu quyền Pages.Exam." |
| 2 | 0:40–1:30 | Trang Đề thi: chỉ vào từng cột (tên đề, file nguồn, số câu, thời gian, lượt làm, điểm TB, trạng thái) | "Danh sách đề hiển thị đủ thông tin quản trị: file nguồn, số câu, lượt làm, điểm trung bình và trạng thái xuất bản." |
| 3 | 1:30–3:00 | Menu **Import từ Word** → chọn `TOEIC_Reading_Test3_Tagged.docx` → bấm Import | "Đề được soạn bằng Word với bộ thẻ chuẩn. Hệ thống đọc file, kiểm tra toàn bộ trước khi lưu: đủ 4 phương án, có đáp án, có gợi ý, đúng thứ tự câu." |
| 4 | 3:00–3:40 | (Tuỳ chọn) Upload một file sai — ví dụ xoá một thẻ ANS — để thấy danh sách lỗi và thông báo **"file chưa được lưu"** | "Nếu file sai dù chỉ một câu, hệ thống huỷ toàn bộ và liệt kê lỗi kèm số câu để giảng viên sửa. Không có dữ liệu rác trong CSDL." |
| 5 | 3:40–5:00 | Trang **Xem trước**: cuộn qua các nhóm câu, chỉ vào bài đọc + ảnh biểu mẫu, đáp án tô xanh, gợi ý tiếng Việt | "Đây là giao diện rà soát: bài đọc gắn đúng nhóm câu, đáp án đúng được tô xanh, gợi ý giải thích tiếng Việt, ảnh cắt từ đề gốc hiển thị đúng vị trí." |
| 6 | 5:00–5:30 | Bấm **Xuất bản đề thi** → badge chuyển "Đã xuất bản" | "Chỉ khi xuất bản, đề mới xuất hiện trên app sinh viên. Đề đã xuất bản vẫn có thể thu hồi." |
| 7 | 5:30–6:10 | Menu **Thống kê**: chỉ vào 4 ô KPI và bảng Top 10 câu sai | "Thống kê lượt làm, điểm trung bình, điểm cao nhất, thời gian làm bài và Top 10 câu nhiều sinh viên sai nhất, kèm phương án nhiễu." |
| 8 | 6:10–7:00 | Chuyển sang app sinh viên: nhập MSSV **2380614888** → danh sách đề → bấm **Bắt đầu làm bài** | "Sinh viên đăng nhập bằng mã số sinh viên, thấy danh sách đề đã xuất bản kèm số câu và thời gian." |
| 9 | 7:00–8:00 | Màn làm bài: chỉ đồng hồ đếm ngược → chọn đáp án B → chỉ tiến độ 1/100 → bấm **Bảng câu hỏi** → chọn vài câu → quay lại | "Đồng hồ 75 phút chạy thời gian thực, chọn đáp án được lưu tạm ngay lên server, bấm lại để huỷ chọn. Bảng câu hỏi cho biết câu nào đã làm, câu nào đánh dấu." |
| 10 | 8:00–8:40 | Bấm **Nộp bài** → hộp thoại xác nhận (hiện số câu chưa làm) → **Nộp bài ngay** | "Trước khi nộp, hệ thống đếm số câu chưa trả lời và yêu cầu xác nhận. Hết giờ thì tự động thu bài." |
| 11 | 8:40–9:40 | Màn **Kết quả**: vòng điểm, đúng/sai/bỏ qua, chi tiết theo Part → cuộn xuống **Xem lại đáp án** | "Chấm điểm tự động: so khoá đáp án, ra điểm và thống kê từng Part. Phần xem lại tô xanh đáp án đúng, tô đỏ đáp án chọn sai, kèm gợi ý." |
| 12 | 9:40–10:10 | Quay lại web giảng viên → **Thống kê** → F5 để thấy lượt làm và Top 10 câu sai vừa cập nhật | "Kết quả vừa nộp được cộng ngay vào thống kê của giảng viên." |
| 13 | 10:10–10:40 | Mở Figma → 2 page Teacher Web / Student Mobile → click 1 frame, mở Layers cho thấy các layer Text | "Giao diện được thiết kế trên Figma với 12 frame, layer text còn chỉnh sửa được, không phải ảnh phẳng." |

## 2. Checklist trước khi upload

- [ ] Video full screen, đọc rõ tên đề và số câu
- [ ] Có cảnh import **thành công** và cảnh import **bị chặn do lỗi**
- [ ] Có cảnh xuất bản → sinh viên thấy đề
- [ ] Có cảnh chọn/huỷ đáp án, cảnh xác nhận nộp
- [ ] Có cảnh kết quả + xem lại đáp án
- [ ] Có cảnh thống kê cập nhật sau khi nộp
- [ ] Có cảnh Figma
- [ ] Đặt tên file: `2380614025_PhamDuyTien_Demo.mp4`, upload Google Drive, để chế độ "Anyone with the link"

## 3. Câu hỏi thường gặp khi bảo vệ

| Câu hỏi | Trả lời ngắn |
|---|---|
| Vì sao 7 bảng? | Vì đề có cấu trúc phân cấp: đề → bài đọc → câu hỏi → phương án; và bài làm → đáp án từng câu. Một bảng không biểu diễn được Part 5 (không có bài đọc) và Part 7 (nhiều tài liệu/bài). |
| Hoán vị câu/phương án có làm sai đáp án không? | Không. ANS lưu theo *key* A–D chứ không theo vị trí; khi trộn chỉ đổi thứ tự hiển thị, key giữ nguyên. |
| Import sai thì sao? | Parser dựng toàn bộ đề trong bộ nhớ, validate hết rồi mới ghi; có lỗi là không ghi gì và trả danh sách lỗi kèm số câu. |
| Chống mất bài khi rớt mạng? | Mỗi lần chọn đáp án gọi SaveAnswers; mở lại bài thi (Start) sẽ trả lại đáp án đã lưu. |
