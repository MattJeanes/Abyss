using Abyss.Web.Data.Options;
using Abyss.Web.Data.SpaceEngineers;
using Abyss.Web.Helpers.Interfaces;
using Microsoft.Extensions.Options;
using RestSharp;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace Abyss.Web.Helpers;

public class SpaceEngineersHelper : ISpaceEngineersHelper
{
    private readonly SpaceEngineersOptions _options;
    private readonly Random _random;
    private readonly RestClient _client;

    public SpaceEngineersHelper(IOptions<SpaceEngineersOptions> options, HttpClient httpClient)
    {
        _options = options.Value;
        _random = new Random();
        _client = new RestClient(httpClient);
    }

    public async Task<List<SpaceEngineersCharacters.Character>> GetCharacters()
    {
        var request = CreateRequest("v1/session/characters", Method.Get);
        var response = await _client.ExecuteAsync<SpaceEngineersResponse<SpaceEngineersCharacters>>(request);
        if (!response.IsSuccessful) { return null; }
        return response.Data.Data.Characters;
    }

    private RestRequest CreateRequest(string resourceLink, Method method, params Tuple<string, string>[] queryParams)
    {
        var methodUrl = $"/vrageremote/{resourceLink}";
        var request = new RestRequest(methodUrl, method);
        var date = DateTime.UtcNow.ToString("r", CultureInfo.InvariantCulture);
        request.AddHeader("Date", date);
        var nonce = _random.Next(0, int.MaxValue).ToString();
        var message = new StringBuilder();
        message.Append(methodUrl);
        if (queryParams.Length > 0)
        {
            message.Append("?");
        }

        for (var i = 0; i < queryParams.Length; i++)
        {
            var param = queryParams[i];
            request.AddQueryParameter(param.Item1, param.Item2);
            message.AppendFormat("{0}={1}", param.Item1, param.Item2);
            if (i != queryParams.Length - 1)
            {
                message.Append("&");
            }
        }

        message.AppendLine();
        message.AppendLine(nonce);
        message.AppendLine(date);
        var messageBuffer = Encoding.UTF8.GetBytes(message.ToString());

        var key = Convert.FromBase64String(_options.ApiKey);
        byte[] computedHash;
        using (var hmac = new HMACSHA1(key))
        {
            computedHash = hmac.ComputeHash(messageBuffer);
        }

        var hash = Convert.ToBase64String(computedHash);
        request.AddHeader("Authorization", string.Format("{0}:{1}", nonce, hash));
        return request;
    }
}
