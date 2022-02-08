namespace Abyss.Web.Helpers.Interfaces;

public interface ICloudflareHelper
{
    Task<CloudflareDNSRecord> GetDNSRecord(string name);
    Task<CloudflareDNSRecord> UpdateDNSRecord(CloudflareDNSRecord record);
}
