using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Abyss.Web.Entities
{
    public class BaseEntity
    {
        [BsonId]
        public ObjectId Id { get; set; }
    }
}
