using Toeic.Roles.Dto;
using System.Collections.Generic;

namespace Toeic.Web.Models.Roles;

public class RoleListViewModel
{
    public IReadOnlyList<PermissionDto> Permissions { get; set; }
}
