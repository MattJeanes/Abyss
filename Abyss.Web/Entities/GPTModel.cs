using MongoDB.Bson;

namespace Abyss.Web.Entities
{
    public class GPTModel : BaseEntity
    {
        public string Name { get; set; }
        public string Identifier { get; set; }
        public ObjectId? Permission { get; set; }
    }
}
