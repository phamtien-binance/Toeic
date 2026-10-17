using Toeic.Configuration.Dto;
using System.Threading.Tasks;

namespace Toeic.Configuration;

public interface IConfigurationAppService
{
    Task ChangeUiTheme(ChangeUiThemeInput input);
}
