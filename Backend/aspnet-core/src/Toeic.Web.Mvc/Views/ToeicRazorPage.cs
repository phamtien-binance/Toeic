using Abp.AspNetCore.Mvc.Views;
using Abp.Runtime.Session;
using Microsoft.AspNetCore.Mvc.Razor.Internal;

namespace Toeic.Web.Views;

public abstract class ToeicRazorPage<TModel> : AbpRazorPage<TModel>
{
    [RazorInject]
    public IAbpSession AbpSession { get; set; }

    protected ToeicRazorPage()
    {
        LocalizationSourceName = ToeicConsts.LocalizationSourceName;
    }
}
