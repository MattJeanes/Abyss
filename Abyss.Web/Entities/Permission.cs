namespace Abyss.Web.Entities;

public class Permission : BaseEntity
{
    public string Identifier { get; set; }

    public string Name { get; set; }

    public string Description { get; set; }

    public List<RolePermission> RolePermissions { get; } = new();

    public List<Role> Roles { get; } = new();
}
