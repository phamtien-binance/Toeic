using Abp.AspNetCore.Mvc.Authorization;
using Toeic.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace Toeic.Web.Controllers;

[AbpMvcAuthorize]
public class HomeController : ToeicControllerBase
{
    public ActionResult Index()
    {
        return View();
    }
}
