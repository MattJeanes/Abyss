using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Abyss.Web.Entities;

[Table("Permissions")]
public class Permission : BaseEntity
{
    public string Identifier { get; set; }

    public string Name { get; set; }

    public string Description { get; set; }

    [JsonIgnore]
    public List<RolePermission> RolePermissions { get; } = new();

    [JsonIgnore]
    public List<Role> Roles { get; } = new();
}
