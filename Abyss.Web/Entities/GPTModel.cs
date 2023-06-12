using System.ComponentModel.DataAnnotations.Schema;

namespace Abyss.Web.Entities;

public class GPTModel : BaseEntity
{
    public string Name { get; set; }

    public string Identifier { get; set; }

    [ForeignKey(nameof(Permission))]
    public int? PermissionId { get; set; }

    public Permission Permission { get; set; }
}
