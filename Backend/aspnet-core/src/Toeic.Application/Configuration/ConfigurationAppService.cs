using Abp.Authorization;
using Abp.Runtime.Session;
using Toeic.Configuration.Dto;
using System.Threading.Tasks;

namespace Toeic.Configuration;

[AbpAuthorize]
public class ConfigurationAppService : ToeicAppServiceBase, IConfigurationAppService
{
    public async Task ChangeUiTheme(ChangeUiThemeInput input)
    {
        await SettingManager.ChangeSettingForUserAsync(AbpSession.ToUserIdentifier(), AppSettingNames.UiTheme, input.Theme);
    }
}
