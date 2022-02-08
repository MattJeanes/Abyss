using System.Text.Json.Serialization;

namespace Abyss.Web.Data.SpaceEngineers;

public class SpaceEngineersMetadata
{
    [JsonPropertyName("apiVersion")]
    public string ApiVersion { get; set; }

    [JsonPropertyName("queryTime")]
    public decimal QueryTime { get; set; }
}
