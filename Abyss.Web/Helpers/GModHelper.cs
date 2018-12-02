using Abyss.Web.Data.GMod;
using Abyss.Web.Helpers.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Abyss.Web.Helpers
{
    public class GModHelper : IGModHelper
    {
        private readonly HttpClient _client;
        private readonly ILogger<GModHelper> _logger;

        public GModHelper(IHttpClientFactory httpClientFactory, ILogger<GModHelper> logger)
        {
            _client = httpClientFactory.CreateClient("gmod");
            _logger = logger;
        }

        public async Task<string> ChangeRank(ChangeRankDTO request)
        {
            return await HandleResponse<string>(await _client.PostAsJsonAsync("rank", request));
        }

        private async Task<T> HandleResponse<T>(HttpResponseMessage message)
        {
            GModResponse<T> resp;
            try
            {
                resp = await message.Content.ReadAsAsync<GModResponse<T>>();
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

        private class GModResponse<T>
        {
            public bool Success { get; set; }
            public string Error { get; set; }
            public T Result { get; set; }
        }
    }
}
