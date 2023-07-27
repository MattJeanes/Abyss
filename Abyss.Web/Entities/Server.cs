using Abyss.Web.Data;
using System.ComponentModel.DataAnnotations.Schema;

namespace Abyss.Web.Entities;

[Table("Servers")]
public class Server : BaseEntity
{
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

    public string ApiBaseUrl { get; set; }

    public string ApiKey { get; set; }
}
