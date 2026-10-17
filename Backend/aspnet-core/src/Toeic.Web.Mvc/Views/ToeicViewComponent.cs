using Abp.AspNetCore.Mvc.ViewComponents;

namespace Toeic.Web.Views;

public abstract class ToeicViewComponent : AbpViewComponent
{
    protected ToeicViewComponent()
    {
        LocalizationSourceName = ToeicConsts.LocalizationSourceName;
    }
}
