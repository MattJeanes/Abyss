using System.Text.Json.Serialization;

namespace Abyss.Web.Data
{
    public class GPTMessage
    {
        [JsonPropertyName("text")]
        public string Text { get; set; }

        [JsonPropertyName("model")]
        public GPTModel Model { get; set; }
    }
}
