import http.cookiejar, urllib.request, urllib.parse, json, os, re, subprocess, uuid

BASE = "http://localhost:5001"
cj = http.cookiejar.CookieJar()
opener = urllib.request.build_opener(urllib.request.HTTPCookieProcessor(cj))
SQLCMD = r"C:\Program Files\Microsoft SQL Server\Client SDK\ODBC\170\Tools\Binn\sqlcmd.exe"

results = []
def check(name, ok, detail=""):
    results.append((name, ok, detail))
    print(("PASS  " if ok else "FAIL  ") + name + ("   " + detail if detail else ""))

TOKEN = ""
def get(path):
    global TOKEN
    r = opener.open(BASE + path, timeout=120)
    html = r.read().decode("utf-8", "replace")
    m = re.search(r'name="__RequestVerificationToken"[^>]*value="([^"]+)"', html)
    if m:
        TOKEN = m.group(1)
    return r.status, html, r.geturl()

def post(path, data, multipart=None):
    if data is not None and TOKEN and "__RequestVerificationToken" not in data:
        data["__RequestVerificationToken"] = TOKEN
    if multipart:
        body, ctype = multipart
    else:
        body = urllib.parse.urlencode(data).encode()
        ctype = "application/x-www-form-urlencoded"
    req = urllib.request.Request(BASE + path, data=body, headers={"Content-Type": ctype})
    r = opener.open(req, timeout=300)
    return r.status, r.read().decode("utf-8", "replace"), r.geturl()

def sql(q):
    p = subprocess.run([SQLCMD, "-S", "localhost", "-E", "-d", "ToeicDb", "-h", "-1", "-W", "-Q",
                        "SET NOCOUNT ON; " + q], capture_output=True, text=True, encoding="utf-8", errors="replace")
    return [l for l in (p.stdout or "").splitlines() if l.strip()]

# ---------------------------------------------------------------- T1 đăng nhập
st, html, _ = get("/Account/Login")
check("T1 trang đăng nhập mở được", st == 200 and "Đăng nhập" in html, f"HTTP {st}")
m = re.search(r'name="__RequestVerificationToken"[^>]*value="([^"]+)"', html)
TOKEN = m.group(1) if m else ""

try:
    st, body, url = post("/Account/Login", {
        "usernameOrEmailAddress": "admin", "password": "123qwe",
        "returnUrl": "/Exam", "rememberMe": "false",
        "__RequestVerificationToken": TOKEN})
except urllib.error.HTTPError as e:
    body = e.read().decode("utf-8", "replace"); st = e.code
    print("   [debug] login HTTP", st, body[:200].replace(chr(10), " "))
try:
    j = json.loads(body)
    target = (j.get("result") or {}).get("targetUrl") or j.get("targetUrl")
except Exception:
    target = None
check("T2 đăng nhập admin -> JSON TargetUrl=/Exam", target == "/Exam", f"target={target}")

# ---------------------------------------------------------------- T3 danh sách
st, html, _ = get("/Exam")
check("T3 danh sách đề (đã đăng nhập)", st == 200 and "TOEIC Reading Test" in html,
      f"HTTP {st}, {len(html)} bytes")
check("T3b có cột trạng thái xuất bản", "Đã xuất bản" in html or "Bản nháp" in html)

# ---------------------------------------------------------------- T4 xem trước
st, html, _ = get("/Exam/Preview?id=4")
imgs = len(re.findall(r'<img[^>]+src="[^"]*uploads', html))
check("T4 xem trước đề 4", st == 200 and "Câu 101" in html, f"HTTP {st}")
check("T4b ảnh trong đề hiển thị (>=7)", imgs >= 7, f"{imgs} ảnh từ /uploads")

# ---------------------------------------------------------------- T5 thống kê
st, html, _ = get("/Exam/Stats?id=4")
check("T5 trang thống kê", st == 200 and ("lượt làm" in html or "Top" in html), f"HTTP {st}")

# ---------------------------------------------------------------- T6 sửa câu: validate
qid = sql("SELECT Id FROM RtQuestions WHERE ExamId=4 AND Number=101")[0].strip()
print("   [info] Question Id của câu 101 (đề 4) =", qid)
before = sql(f"SELECT Hint FROM RtQuestions WHERE Id={qid}")[0].strip()
st, body, url2 = post("/Exam/EditQuestion", {
    "id": "4", "questionId": qid, "stem": "st", "hint": "hint",
    "answerKey": "A", "optA": "a", "optB": "", "optC": "c", "optD": "d"})
after = sql(f"SELECT Hint FROM RtQuestions WHERE Id={qid}")[0].strip()
check("T6 chặn lưu khi thiếu phương án", before == after, "DB không đổi")

# ---------------------------------------------------------------- T7 sửa câu: lưu thật
newhint = "Gợi ý kiểm thử tự động " + uuid.uuid4().hex[:6]
post("/Exam/EditQuestion", {
    "id": "4", "questionId": qid, "stem": "The manager asked the clerk to ___ the form.",
    "hint": newhint, "answerKey": "A", "optA": "opens", "optB": "open", "optC": "opening", "optD": "opened"})
saved = sql(f"SELECT Hint FROM RtQuestions WHERE Id={qid}")[0].strip()
optA = sql(f"SELECT Text FROM RtQuestionOptions WHERE QuestionId={qid} AND OptionKey='A'")[0].strip()
check("T7 lưu câu hỏi -> DB đổi", saved == newhint, f"hint='{saved[:28]}...'")
check("T7b phương án A giữ nguyên", optA == "opens", f"A='{optA}'")
# khôi phục
post("/Exam/EditQuestion", {
    "id": "4", "questionId": qid, "stem": "The manager asked the clerk to ___ the form.",
    "hint": before, "answerKey": "A", "optA": "opens", "optB": "open", "optC": "opening", "optD": "opened"})
restored = sql(f"SELECT Hint FROM RtQuestions WHERE Id={qid}")[0].strip()
check("T7c khôi phục gợi ý gốc", restored == before)

# ---------------------------------------------------------------- T8 xuất bản / thu hồi
post("/Exam/Unpublish", {"id": "5"})
st5 = sql("SELECT Status FROM RtExams WHERE Id=5")[0].strip()
post("/Exam/Publish", {"id": "5"})
st5b = sql("SELECT Status FROM RtExams WHERE Id=5")[0].strip()
check("T8 thu hồi -> Status 1", st5 == "1", f"status={st5}")
check("T8b xuất bản lại -> Status 2", st5b == "2", f"status={st5b}")

# ---------------------------------------------------------------- T9 trang import
st, html, _ = get("/Exam/Import")
check("T9 trang import mở được", st == 200 and "docx" in html.lower(), f"HTTP {st}")

# ---------------------------------------------------------------- T10 import file thật
docx_path = r"C:\Users\zzpha\Downloads\kiemtraa\Files\TOEIC_Reading_Test3_Tagged.docx"
boundary = "----HermesBoundary" + uuid.uuid4().hex
with open(docx_path, "rb") as f:
    file_bytes = f.read()
parts = []
def field(name, value):
    parts.append(("--" + boundary + "\r\nContent-Disposition: form-data; name=\"" + name + "\"\r\n\r\n" + value + "\r\n").encode())
field("__RequestVerificationToken", TOKEN)
field("Title", "Đề kiểm thử tự động")
field("DurationMinutes", "75")
head = ("--" + boundary + "\r\nContent-Disposition: form-data; name=\"file\"; filename=\"kiemtra.docx\"\r\n"
        "Content-Type: application/vnd.openxmlformats-officedocument.wordprocessingml.document\r\n\r\n").encode()
tail = ("\r\n--" + boundary + "--\r\n").encode()
body = b"".join(parts) + head + file_bytes + tail
st, html, final = post("/Exam/Import", None,
                       multipart=(body, "multipart/form-data; boundary=" + boundary))
newid = None
m = re.search(r"/Exam/Preview(?:\?id=|/)(\d+)", final)
if m:
    newid = int(m.group(1))
check("T10 import .docx qua web", newid is not None, f"redirect -> {final[:60]}")
if newid:
    qn = sql(f"SELECT COUNT(*) FROM RtQuestions WHERE ExamId={newid}")[0].strip()
    im = sql(f"SELECT COUNT(*) FROM RtPassageDocuments WHERE ImagePath IS NOT NULL AND PassageId IN (SELECT Id FROM RtPassages WHERE ExamId={newid})")[0].strip()
    check("T10b đề mới có 100 câu", qn == "100", f"{qn} câu")
    check("T10c ảnh trích ra đĩa", im == "7", f"{im} ảnh")
    files = os.listdir(rf"C:\Users\zzpha\Downloads\kiemtraa\Backend\uploads\exams\{newid}") if os.path.isdir(rf"C:\Users\zzpha\Downloads\kiemtraa\Backend\uploads\exams\{newid}") else []
    check("T10d thư mục uploads có 7 tệp", len(files) == 7, f"{len(files)} tệp")
    # dọn dẹp
    post("/Exam/Delete", {"id": str(newid)})
    left = sql(f"SELECT COUNT(*) FROM RtExams WHERE Id={newid} AND IsDeleted=0")[0].strip()
    check("T11 xoá đề kiểm thử", left == "0", "đã xoá")

# ---------------------------------------------------------------- T12 chặn khi chưa đăng nhập
anon = urllib.request.build_opener()  # không cookie
try:
    r = anon.open(BASE + "/Exam", timeout=60)
    code, loc = r.status, r.geturl()
except urllib.error.HTTPError as e:
    code, loc = e.code, ""
check("T12 chưa đăng nhập bị chặn", "login" in loc.lower() or code in (302, 401), f"-> {loc[:60] or code}")

print("\n===== TỔNG KẾT:", sum(1 for _, ok, _ in results if ok), "/", len(results), "đạt =====")
