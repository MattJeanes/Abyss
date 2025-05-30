﻿using Abyss.Web.Data.Options;
using Abyss.Web.Helpers.Interfaces;
using Microsoft.Extensions.Options;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Abyss.Web.Helpers;

public class CloudflareHelper(IHttpClientFactory httpClientFactory, IOptions<CloudflareOptions> options) : ICloudflareHelper
{
    private readonly HttpClient _client = httpClientFactory.CreateClient("cloudflare");
    private readonly CloudflareOptions _options = options.Value;

    public async Task<(string zoneId, CloudflareDNSRecord dnsRecord)> GetDNSRecord(string name)
    {
        var zone = await GetZone(_options.Zone);
        if (zone == null) { return (null, null); }
        return (zone.Id, await GetDNSRecord(zone.Id, name));
    }

    public async Task<CloudflareDNSRecord> UpdateDNSRecord(string zoneId, CloudflareDNSRecord record)
    {
        var resp = await _client.PutAsync($"zones/{zoneId}/dns_records/{record.Id}", new StringContent(JsonSerializer.Serialize(record), Encoding.UTF8, "application/json"));
        var newRecord = await GetResponse<CloudflareDNSRecord>(resp);
        return newRecord;
    }

    private async Task<CloudflareZone> GetZone(string name)
    {
        var resp = await _client.GetAsync($"zones?name={name}");
        var zones = await JsonSerializer.DeserializeAsync<CloudflareResponse<List<CloudflareZone>>>(await resp.Content.ReadAsStreamAsync(), Program.JsonSerializerOptions);
        var zone = zones!.Result.FirstOrDefault();
        return zone;
    }

    private async Task<CloudflareDNSRecord> GetDNSRecord(string zoneId, string name)
    {
        var resp = await _client.GetAsync($"zones/{zoneId}/dns_records?name={name}");
        var records = await GetResponse<List<CloudflareDNSRecord>>(resp);
        return records.FirstOrDefault();
    }

    private async Task<T> GetResponse<T>(HttpResponseMessage message)
    {
        var resp = await JsonSerializer.DeserializeAsync<CloudflareResponse<T>>(await message.Content.ReadAsStreamAsync(), Program.JsonSerializerOptions);
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
    public string Id { get; set; }

    public string Name { get; set; }
}

public class CloudflareDNSRecord
{
    [JsonPropertyName("id")]
    public string Id { get; set; }

    [JsonPropertyName("type")]
    public string Type { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("content")]
    public string Content { get; set; }

    [JsonPropertyName("proxiable")]
    public bool Proxiable { get; set; }

    [JsonPropertyName("proxied")]
    public bool Proxied { get; set; }

    [JsonPropertyName("ttl")]
    public int Ttl { get; set; }

    [JsonPropertyName("locked")]
    public bool Locked { get; set; }

    [JsonPropertyName("created_on")]
    public DateTime CreatedOn { get; set; }

    [JsonPropertyName("modified_on")]
    public DateTime ModifiedOn { get; set; }

    [JsonPropertyName("data")]
    public object Data { get; set; }
}
