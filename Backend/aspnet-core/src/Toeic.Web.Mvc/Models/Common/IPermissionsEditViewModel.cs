using Toeic.Roles.Dto;
using System.Collections.Generic;

namespace Toeic.Web.Models.Common;

public interface IPermissionsEditViewModel
{
    List<FlatPermissionDto> Permissions { get; set; }
}