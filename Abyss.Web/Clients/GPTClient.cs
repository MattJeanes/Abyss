using Abyss.Web.Clients.Interfaces;
using Abyss.Web.Data;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;


namespace Abyss.Web.Clients
{
    public class GPTClient : IGPTClient
    {
        private readonly HttpClient _httpClient;

        public GPTClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<GPTMessage> Generate(string message)
        {
            var httpContent = new StringContent(JsonSerializer.Serialize(new GPTMessage { Text = message }));
            httpContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            var resp = await _httpClient.PostAsync("/api/generate", httpContent);
            resp.EnsureSuccessStatusCode();
            var generated = await JsonSerializer.DeserializeAsync<GPTMessage>(await resp.Content.ReadAsStreamAsync());
            return generated;
        }
    }
}
