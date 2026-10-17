using Abp.Application.Services;
using Toeic.Sessions.Dto;
using System.Threading.Tasks;

namespace Toeic.Sessions;

public interface ISessionAppService : IApplicationService
{
    Task<GetCurrentLoginInformationsOutput> GetCurrentLoginInformations();
}
