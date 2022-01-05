using MongoDB.Bson;

namespace Abyss.Web.Data;

public struct GPTRequest
{
    public string Text;

    public ObjectId ModelId;
}
