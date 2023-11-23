using Abyss.Web.Data;
using Abyss.Web.Data.SpaceEngineers;
using Abyss.Web.Entities;
using Abyss.Web.Helpers.Interfaces;
using RestSharp;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace Abyss.Web.Helpers;

public class SpaceEngineersHelper(HttpClient httpClient) : ISpaceEngineersHelper
{
    private readonly Random _random = new Random();
    private readonly RestClient _client = new RestClient(httpClient);

    public async Task<List<SpaceEngineersCharacters.Character>> GetCharacters(Server server)
    {
        if (server.Type != ServerType.SpaceEngineers)
        {
            throw new InvalidOperationException(nameof(server.Type));
        }
        var request = CreateRequest(server, "v1/session/characters", Method.Get);
        var response = await HandleResponse<SpaceEngineersCharacters>(request);
        return response.Characters;
    }

    private async Task<T> HandleResponse<T>(RestRequest request)
    {
        var response = await _client.ExecuteAsync<SpaceEngineersResponse<T>>(request);
        if (!response.IsSuccessful)
        {
            if (response.ErrorException != null)
            {
                throw response.ErrorException;
            }
            if (!string.IsNullOrEmpty(response.ErrorMessage))
            {
                throw new Exception(response.ErrorMessage);
            }
            else
            {
                throw new Exception($"Failed to process request {request.Resource}");
            }
        }
        return response.Data.Data;
    }

    private RestRequest CreateRequest(Server server, string resourceLink, Method method, params Tuple<string, string>[] queryParams)
    {
        var methodUrl = $"/vrageremote/{resourceLink}";
        var baseUri = new Uri(server.ApiBaseUrl);
        var request = new RestRequest(new Uri(baseUri, methodUrl), method);
        var date = DateTime.UtcNow.ToString("r", CultureInfo.InvariantCulture);
        request.AddHeader("Date", date);
        var nonce = _random.Next(0, int.MaxValue).ToString();
        var message = new StringBuilder();
        message.Append(methodUrl);
        if (queryParams.Length > 0)
        {
            message.Append('?');
        }

        for (var i = 0; i < queryParams.Length; i++)
        {
            var param = queryParams[i];
            request.AddQueryParameter(param.Item1, param.Item2);
            message.AppendFormat("{0}={1}", param.Item1, param.Item2);
            if (i != queryParams.Length - 1)
            {
                message.Append('&');
            }
        }

        message.Append("\r\n");
        message.Append($"{nonce}\r\n");
        message.Append($"{date}\r\n");
        var messageBuffer = Encoding.UTF8.GetBytes(message.ToString());

        var key = Convert.FromBase64String(server.ApiKey);
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
