using Abyss.Web.Data.GMod;
using Abyss.Web.Data.Options;
using Abyss.Web.Helpers.Interfaces;
using Microsoft.Extensions.Options;
using System.Text;
using System.Text.Json;

namespace Abyss.Web.Helpers;

public class GModHelper : IGModHelper
{
    private readonly HttpClient _client;
    private readonly ILogger<GModHelper> _logger;
    private readonly GModOptions _options;

    public GModHelper(
        IHttpClientFactory httpClientFactory,
        ILogger<GModHelper> logger,
        IOptions<GModOptions> options
        )
    {
        _client = httpClientFactory.CreateClient("gmod");
        _logger = logger;
        _options = options.Value;
    }

    public async Task<string> ChangeRank(ChangeRankDTO request)
    {
        return await HandleResponse<string>(await _client.PostAsync("rank", new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json")));
    }

    public bool IsActive()
    {
        return _options.Active;
    }

    private async Task<T> HandleResponse<T>(HttpResponseMessage message)
    {
        GModResponse<T> resp;
        try
        {
            resp = await JsonSerializer.DeserializeAsync<GModResponse<T>>(await message.Content.ReadAsStreamAsync(), Startup.JsonSerializerOptions);
        }
        catch (Exception e)
        {
            throw new Exception("Failed to contact GMod server", e);
        }
        if (!resp.Success)
        {
            throw new Exception(resp.Error);
        }
        return resp.Result;
    }

    public struct GModResponse<T>
    {
        public bool Success;
        public string Error;
        public T Result;
    }
}
