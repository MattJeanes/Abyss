using Abyss.Web.Data.Options;
using Abyss.Web.Helpers.Interfaces;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Abyss.Web.Helpers
{
    public class CloudflareHelper : ICloudflareHelper
    {
        private readonly HttpClient _client;
        private readonly CloudflareOptions _options;

        public CloudflareHelper(IHttpClientFactory httpClientFactory, IOptions<CloudflareOptions> options)
        {
            _client = httpClientFactory.CreateClient("cloudflare");
            _options = options.Value;
        }

        public async Task<CloudflareDNSRecord> GetDNSRecord(string name)
        {
            var zone = await GetZone(_options.Zone);
            return await GetDNSRecord(zone.Id, name);
        }

        public async Task<CloudflareDNSRecord> UpdateDNSRecord(CloudflareDNSRecord record)
        {
            var resp = await _client.PutAsJsonAsync($"zones/{record.ZoneId}/dns_records/{record.Id}", record);
            var newRecord = await GetResponse<CloudflareDNSRecord>(resp);
            return newRecord;
        }

        private async Task<CloudflareZone> GetZone(string name)
        {
            var resp = await _client.GetAsync($"zones?name={name}");
            var zones = await resp.Content.ReadAsAsync<CloudflareResponse<List<CloudflareZone>>>();
            var zone = zones.Result.FirstOrDefault();
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
            var resp = await message.Content.ReadAsAsync<CloudflareResponse<T>>();
            if (!resp.Success)
            {
                throw new Exception(resp.Errors.Any() ? string.Join(", ", resp.Errors.Select(x => x.Message)) : "Unknown Cloudflare error");
            }
            return resp.Result;
        }
    }

    public class CloudflareResponse<T>
    {
        public bool Success { get; set; }
        public List<CloudflareError> Errors { get; set; }
        public List<string> Messages { get; set; }
        public T Result { get; set; }
    }

    public class CloudflareError
    {
        public int Code { get; set; }
        public string Message { get; set; }
    }

    public class CloudflareZone
    {
        public string Id { get; set; }
        public string Name { get; set; }
    }

    public class CloudflareDNSRecord
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "type")]
        public string Type { get; set; }

        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "content")]
        public string Content { get; set; }

        [JsonProperty(PropertyName = "proxiable")]
        public bool Proxiable { get; set; }

        [JsonProperty(PropertyName = "proxied")]
        public bool Proxied { get; set; }

        [JsonProperty(PropertyName = "ttl")]
        public int Ttl { get; set; }

        [JsonProperty(PropertyName = "locked")]
        public bool Locked { get; set; }

        [JsonProperty(PropertyName = "zone_id")]
        public string ZoneId { get; set; }

        [JsonProperty(PropertyName = "zone_name")]
        public string ZoneName { get; set; }

        [JsonProperty(PropertyName = "created_on")]
        public DateTime Created_on { get; set; }

        [JsonProperty(PropertyName = "modified_on")]
        public DateTime ModifiedOn { get; set; }

        [JsonProperty(PropertyName = "data")]
        public object Data { get; set; }
    }
}
