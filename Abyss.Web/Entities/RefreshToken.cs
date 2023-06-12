using System.ComponentModel.DataAnnotations.Schema;

namespace Abyss.Web.Entities;

public class RefreshToken : BaseEntity
{
    public DateTime FromDate { get; set; }

    public DateTime Expiry { get; set; }

    public bool Revoked { get; set; }

    [ForeignKey(nameof(User))]
    public int UserId { get; set; }

    public User User { get; set; }
}
