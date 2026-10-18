using Abp.AspNetCore.Mvc.Authorization;
using Toeic.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace Toeic.Web.Controllers;

[AbpMvcAuthorize]
public class HomeController : ToeicControllerBase
{
    public ActionResult Index()
    {
        // Trang chủ mặc định của ABP (About) không dùng trong đồ án:
        // đưa giảng viên thẳng vào danh sách đề thi.
        return Redirect("/Exam");
    }
}
