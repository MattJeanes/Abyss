using Abyss.Web.Data;

namespace Abyss.Web.Entities
{
    public class Server : BaseEntity
    {
        public string Tag { get; set; }
        public int? SnapshotId { get; set; }
        public string Size { get; set; }
        public string Resize { get; set; }
        public string Region { get; set; }
        public int? DropletId { get; set; }
        public ServerStatus StatusId { get; set; }
        public string IPAddress { get; set; }
        public string DNSRecord { get; set; }
        public string Name { get; set; }
    }
}
