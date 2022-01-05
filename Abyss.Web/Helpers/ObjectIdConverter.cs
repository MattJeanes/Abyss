using MongoDB.Bson;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Abyss.Web.Helpers;

public class ObjectIdConverter : JsonConverter<ObjectId>
{
    public override ObjectId Read(
       ref Utf8JsonReader reader,
       Type typeToConvert,
       JsonSerializerOptions options) =>
           ObjectId.Parse(reader.GetString());

    public override void Write(
        Utf8JsonWriter writer,
        ObjectId dateTimeValue,
        JsonSerializerOptions options) =>
            writer.WriteStringValue(dateTimeValue.ToString());
}
