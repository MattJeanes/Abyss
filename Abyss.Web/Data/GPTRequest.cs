using MongoDB.Bson;

namespace Abyss.Web.Data
{
    public class GPTRequest
    {
        public string Text { get; set; }

        public ObjectId ModelId { get; set; }
    }
}
