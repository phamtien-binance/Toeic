using Abp.AspNetCore.Mvc.Controllers;
using Abp.IdentityFramework;
using Microsoft.AspNetCore.Identity;

namespace Toeic.Controllers
{
    public abstract class ToeicControllerBase : AbpController
    {
        protected ToeicControllerBase()
        {
            LocalizationSourceName = ToeicConsts.LocalizationSourceName;
        }

        protected void CheckErrors(IdentityResult identityResult)
        {
            identityResult.CheckErrors(LocalizationManager);
        }
    }
}
