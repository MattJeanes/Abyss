using MongoDB.Bson;

namespace Abyss.Web.Entities;

public class User : BaseEntity
{
    public string Name { get; set; }
    public Dictionary<string, string> Authentication { get; set; }
    public ObjectId RoleId { get; set; }
}
