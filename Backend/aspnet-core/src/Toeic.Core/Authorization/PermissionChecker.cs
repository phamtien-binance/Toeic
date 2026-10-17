using Abp.Authorization;
using Toeic.Authorization.Roles;
using Toeic.Authorization.Users;

namespace Toeic.Authorization;

public class PermissionChecker : PermissionChecker<Role, User>
{
    public PermissionChecker(UserManager userManager)
        : base(userManager)
    {
    }
}
