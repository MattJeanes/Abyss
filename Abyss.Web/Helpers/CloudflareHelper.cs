using Abyss.Web.Data.Options;
using Abyss.Web.Helpers.Interfaces;
using Microsoft.Extensions.Options;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Abyss.Web.Helpers;

public class CloudflareHelper : ICloudflareHelper
{
    private readonly HttpClient _client;
    private readonly CloudflareOptions _options;

    public CloudflareHelper(IHttpClientFactory httpClientFactory, IOptions<CloudflareOptions> options)
    {
        _client = httpClientFactory.CreateClient("cloudflare");
        _options = options.Value;
    }

    public async Task<CloudflareDNSRecord?> GetDNSRecord(string name)
    {
        var zone = await GetZone(_options.Zone);
        if (zone == null) { return null; }
        return await GetDNSRecord(zone.Id, name);
    }

    public async Task<CloudflareDNSRecord?> UpdateDNSRecord(CloudflareDNSRecord record)
    {
        var resp = await _client.PutAsync($"zones/{record.ZoneId}/dns_records/{record.Id}", new StringContent(JsonSerializer.Serialize(record), Encoding.UTF8, "application/json"));
        var newRecord = await GetResponse<CloudflareDNSRecord>(resp);
        return newRecord;
    }

    private async Task<CloudflareZone?> GetZone(string name)
    {
        var resp = await _client.GetAsync($"zones?name={name}");
        var zones = await JsonSerializer.DeserializeAsync<CloudflareResponse<List<CloudflareZone>>>(await resp.Content.ReadAsStreamAsync(), Startup.JsonSerializerOptions);
        var zone = zones!.Result.FirstOrDefault();
        return zone;
    }

    private async Task<CloudflareDNSRecord?> GetDNSRecord(string zoneId, string name)
    {
        var resp = await _client.GetAsync($"zones/{zoneId}/dns_records?name={name}");
        var records = await GetResponse<List<CloudflareDNSRecord>>(resp);
        return records.FirstOrDefault();
    }

    private async Task<T> GetResponse<T>(HttpResponseMessage message)
    {
        var resp = await JsonSerializer.DeserializeAsync<CloudflareResponse<T>>(await message.Content.ReadAsStreamAsync(), Startup.JsonSerializerOptions);
        if (!resp!.Success)
        {
            throw new Exception(resp.Errors.Any() ? string.Join(", ", resp.Errors.Select(x => x.Message)) : "Unknown Cloudflare error");
        }
        return resp.Result;
    }
}

public struct CloudflareResponse<T>
{
    public bool Success;
    public List<CloudflareError> Errors;
    public List<string> Messages;
    public T Result;
}

public struct CloudflareError
{
    public int Code;
    public string Message;
}

public class CloudflareZone
{
    [NotNull]
    public string? Id { get; set; }

    [NotNull]
    public string? Name { get; set; }
}

public class CloudflareDNSRecord
{
    [JsonPropertyName("id"), NotNull]
    public string? Id { get; set; }

    [JsonPropertyName("type"), NotNull]
    public string? Type { get; set; }

    [JsonPropertyName("name"), NotNull]
    public string? Name { get; set; }

    [JsonPropertyName("content"), NotNull]
    public string? Content { get; set; }

    [JsonPropertyName("proxiable")]
    public bool Proxiable { get; set; }

    [JsonPropertyName("proxied")]
    public bool Proxied { get; set; }

    [JsonPropertyName("ttl")]
    public int Ttl { get; set; }

    [JsonPropertyName("locked")]
    public bool Locked { get; set; }

    [JsonPropertyName("zone_id"), NotNull]
    public string? ZoneId { get; set; }

    [JsonPropertyName("zone_name"), NotNull]
    public string? ZoneName { get; set; }

    [JsonPropertyName("created_on")]
    public DateTime Created_on { get; set; }

    [JsonPropertyName("modified_on")]
    public DateTime ModifiedOn { get; set; }

    [JsonPropertyName("data"), NotNull]
    public object? Data { get; set; }
}
