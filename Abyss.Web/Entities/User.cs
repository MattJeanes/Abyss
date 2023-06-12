using System.ComponentModel.DataAnnotations.Schema;

namespace Abyss.Web.Entities;

public class User : BaseEntity
{
    public string Name { get; set; }

    public List<UserAuthentication> Authentications { get; } = new();

    [ForeignKey(nameof(Role))]
    public int? RoleId { get; set; }

    public Role Role { get; set; }
}
