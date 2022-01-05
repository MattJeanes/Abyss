using MongoDB.Bson;

namespace Abyss.Web.Entities;

public class RefreshToken : BaseEntity
{
    public DateTime FromDate { get; set; }
    public DateTime Expiry { get; set; }
    public bool Revoked { get; set; }
    public ObjectId UserId { get; set; }
}
