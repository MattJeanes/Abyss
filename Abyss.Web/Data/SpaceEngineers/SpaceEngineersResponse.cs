using System.Text.Json.Serialization;

namespace Abyss.Web.Data.SpaceEngineers;

public class SpaceEngineersResponse<T>
{
    [JsonPropertyName("data")]
    public T Data { get; set; }

    [JsonPropertyName("meta")]
    public SpaceEngineersMetadata Meta { get; set; }
}
