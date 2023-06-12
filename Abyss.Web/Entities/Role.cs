using System.ComponentModel.DataAnnotations.Schema;

namespace Abyss.Web.Entities;

[Table("Roles")]
public class Role : BaseEntity
{
    public string Name { get; set; }

    public List<RolePermission> RolePermissions { get; } = new();

    public List<Permission> Permissions { get; } = new();
}
