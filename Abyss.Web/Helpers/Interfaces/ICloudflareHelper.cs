namespace Abyss.Web.Helpers.Interfaces;

public interface ICloudflareHelper
{
    Task<(string zoneId, CloudflareDNSRecord dnsRecord)> GetDNSRecord(string name);
    Task<CloudflareDNSRecord> UpdateDNSRecord(string zoneId, CloudflareDNSRecord record);
}
