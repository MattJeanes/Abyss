using Abyss.Web.Data;

namespace Abyss.Web.Entities;

public class Server : BaseEntity
{
    public string Tag { get; set; }
    public long? SnapshotId { get; set; }
    public string Size { get; set; }
    public string Resize { get; set; }
    public string Region { get; set; }
    public long? DropletId { get; set; }
    public ServerStatus StatusId { get; set; }
    public string IPAddress { get; set; }
    public string DNSRecord { get; set; }
    public string Name { get; set; }
    public int? RemindAfterMinutes { get; set; }
    public int? ReminderIntervalMinutes { get; set; }
    public DateTime? NextReminder { get; set; }
    public CloudType CloudType { get; set; }
    public string ResourceId { get; set; }
    public string Alias { get; set; }
    public ServerType Type { get; set; }
}
