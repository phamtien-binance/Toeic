using Abp.Application.Services;
using Toeic.Authorization.Accounts.Dto;
using System.Threading.Tasks;

namespace Toeic.Authorization.Accounts;

public interface IAccountAppService : IApplicationService
{
    Task<IsTenantAvailableOutput> IsTenantAvailable(IsTenantAvailableInput input);

    Task<RegisterOutput> Register(RegisterInput input);
}
