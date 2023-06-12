using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace Abyss.Web.Entities;

[PrimaryKey(nameof(RoleId), nameof(PermissionId))]
public class RolePermission
{
    [ForeignKey(nameof(Role))]
    public int RoleId { get; set; }

    public Role Role { get; set; }

    [ForeignKey(nameof(Permission))]
    public int PermissionId { get; set; }

    public Permission Permission { get; set; }
}
