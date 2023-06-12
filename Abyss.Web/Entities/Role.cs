namespace Abyss.Web.Entities;

public class Role : BaseEntity
{
    public string Name { get; set; }

    public List<RolePermission> RolePermissions { get; } = new();

    public List<Permission> Permissions { get; } = new();
}
