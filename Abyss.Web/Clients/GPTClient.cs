using Abyss.Web.Clients.Interfaces;
using Abyss.Web.Data;
using System.Net.Http.Headers;
using System.Text.Json;

namespace Abyss.Web.Clients;

public class GPTClient : IGPTClient
{
    private readonly HttpClient _httpClient;

    public GPTClient(HttpClient httpClient) => _httpClient = httpClient;

    public async Task<GPTResponse> Generate(string model, string message, decimal temperature, decimal top_p)
    {
        var serializerOptions = new JsonSerializerOptions();
        serializerOptions.PropertyNameCaseInsensitive = true;
        serializerOptions.IncludeFields = true;
        var json = JsonSerializer.Serialize(new { text = message, model, temperature, top_p }, serializerOptions);
        var httpContent = new StringContent(json);
        httpContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
        var resp = await _httpClient.PostAsync("/api/generate", httpContent);
        resp.EnsureSuccessStatusCode();
        var generated = await JsonSerializer.DeserializeAsync<GPTResponse>(await resp.Content.ReadAsStreamAsync(), serializerOptions);
        return generated!;
    }
}
