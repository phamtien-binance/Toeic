using Toeic.Roles.Dto;
using System.Collections.Generic;

namespace Toeic.Web.Models.Users;

public class UserListViewModel
{
    public IReadOnlyList<RoleDto> Roles { get; set; }
}
