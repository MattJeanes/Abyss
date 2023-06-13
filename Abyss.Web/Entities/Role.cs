using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Abyss.Web.Entities;

[Table("Roles")]
public class Role : BaseEntity
{
    public string Name { get; set; }

    [JsonIgnore]
    public List<RolePermission> RolePermissions { get; } = new();

    public List<Permission> Permissions { get; } = new();
}
