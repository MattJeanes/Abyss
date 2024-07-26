using Abyss.Web.Data.Options;
using Abyss.Web.Entities;
using Abyss.Web.Helpers.Interfaces;
using Abyss.Web.Logging;
using Microsoft.Extensions.Options;
using System.ComponentModel;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Sockets;
using System.Text;
using System.Text.Json.Serialization;

namespace Abyss.Web.Helpers;

public class OvhHelper(HttpClient client, IOptions<OvhOptions> options) : IOvhHelper
{
    private readonly HttpClient _client = client;
    private readonly OvhOptions _options = options.Value;

    public async Task StartServer(Server server, TaskLogger logger)
    {
        var (projectId, instanceId) = GetPublicCloudResourceId(server.ResourceId);
        logger.LogInformation($"Starting OVH public cloud instance id {instanceId} in project id {projectId}");
        var req = new HttpRequestMessage(HttpMethod.Post, $"/v1/cloud/project/{projectId}/instance/{instanceId}/start");
        await AddHeaders(req.Headers);
        var res = await _client.SendAsync(req);
        res.EnsureSuccessStatusCode();
        logger.LogInformation($"OVH public cloud instance started");
    }

    public async Task StopServer(Server server, TaskLogger logger)
    {
        var (projectId, instanceId) = GetPublicCloudResourceId(server.ResourceId);
        logger.LogInformation($"Stopping OVH public cloud instance id {instanceId} in project id {projectId}");
        var req = new HttpRequestMessage(HttpMethod.Post, $"/v1/cloud/project/{projectId}/instance/{instanceId}/stop");
        await AddHeaders(req.Headers);
        var res = await _client.SendAsync(req);
        res.EnsureSuccessStatusCode();
        logger.LogInformation($"OVH public cloud instance stopped");
    }

    public async Task RestartServer(Server server, TaskLogger logger, ServerRebootType type = ServerRebootType.Soft)
    {
        var (projectId, instanceId) = GetPublicCloudResourceId(server.ResourceId);
        logger.LogInformation($"{type} restarting OVH public cloud instance id {instanceId} in project id {projectId}");
        var typeStr = EnumHelper.GetEnumDescription(type);
        var content = new StringContent($$"""
            {"type": "{{typeStr}}"}
        """, Encoding.UTF8, "application/json");
        var req = new HttpRequestMessage(HttpMethod.Post, $"/v1/cloud/project/{projectId}/instance/{instanceId}/reboot");
        req.Content = content;
        await AddHeaders(req.Headers);
        var res = await _client.SendAsync(req);
        res.EnsureSuccessStatusCode();
        logger.LogInformation($"OVH public cloud instance restarted");
    }

    public async Task<string> GetServerIpAddress(Server server)
    {
        var (projectId, instanceId) = GetPublicCloudResourceId(server.ResourceId);
        var req = new HttpRequestMessage(HttpMethod.Get, $"/v1/cloud/project/{projectId}/instance/{instanceId}/interface");
        await AddHeaders(req.Headers);
        var res = await _client.SendAsync(req);
        res.EnsureSuccessStatusCode();
        var interfaceResponse = await res.Content.ReadFromJsonAsync<List<OvhInterfaceResponse>>();
        var serverInterface = interfaceResponse.FirstOrDefault();
        if (serverInterface == null)
        {
            throw new InvalidOperationException($"OVH instance id {instanceId} in project id {projectId} has no interfaces");
        }
        var ip = serverInterface.FixedIps.Select(x => IPAddress.Parse(x.Ip)).FirstOrDefault(x => x.AddressFamily == AddressFamily.InterNetwork).ToString();
        if (ip == null)
        {
            throw new InvalidOperationException($"OVH interface id {serverInterface.Id} in instance id {instanceId} in project id {projectId} has no IPv4 address");
        }
        return ip;
    }

    private (string projectId, string instanceId) GetPublicCloudResourceId(string resourceId)
    {
        var resourceSplit = resourceId.Split('/');
        if (resourceSplit.Length < 2)
        {
            throw new InvalidOperationException($"Invalid OVH resource id {resourceId}");
        }
        var type = resourceSplit[1];
        if (type != "public-cloud")
        {
            throw new InvalidOperationException($"Invalid OVH resource type {type}");
        }
        if (resourceSplit.Length != 7)
        {
            throw new InvalidOperationException($"Invalid OVH public-cloud resource id: {resourceId}");
        }

        var projectId = resourceSplit[4];
        var instanceId = resourceSplit[6];

        return (projectId, instanceId);
    }

    private async Task AddHeaders(HttpRequestHeaders headers)
    {
        var formData = new List<KeyValuePair<string, string>>
        {
            new KeyValuePair<string, string>("grant_type", "client_credentials"),
            new KeyValuePair<string, string>("client_id", _options.ClientId),
            new KeyValuePair<string, string>("client_secret", _options.ClientSecret),
            new KeyValuePair<string, string>("scope", "all")
        };
        var content = new FormUrlEncodedContent(formData);
        var response = await _client.PostAsync(_options.OAuthUrl, content);
        response.EnsureSuccessStatusCode();
        var oauthResponse = await response.Content.ReadFromJsonAsync<OvhAuthResponse>();

        headers.Add("Accept", "application/json");
        headers.Add("Authorization", $"Bearer {oauthResponse.AccessToken}");
    }

    public enum ServerRebootType
    {
        [Description("hard")]
        Hard,

        [Description("soft")]
        Soft
    }

    private class OvhAuthResponse
    {
        [JsonPropertyName("access_token")]
        public string AccessToken { get; set; }

        [JsonPropertyName("token_type")]
        public string TokenType { get; set; }

        [JsonPropertyName("expires_in")]
        public int ExpiresIn { get; set; }

        [JsonPropertyName("scope")]
        public string Scope { get; set; }
    }

    public class OvhInterfaceResponse
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("macAddress")]
        public string MacAddress { get; set; }

        [JsonPropertyName("state")]
        public string State { get; set; }

        [JsonPropertyName("fixedIps")]
        public FixedIp[] FixedIps { get; set; }

        [JsonPropertyName("networkId")]
        public string NetworkId { get; set; }

        [JsonPropertyName("type")]
        public string Type { get; set; }

        public class FixedIp
        {
            [JsonPropertyName("subnetId")]
            public string SubnetId { get; set; }

            [JsonPropertyName("ip")]
            public string Ip { get; set; }
        }
    }
}