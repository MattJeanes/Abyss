using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Abyss.Web.Data
{
    public class ClientUser
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public Dictionary<string, string> Authentication { get; set; }
        public string RoleId { get; set; }
        public List<string> Permissions { get; set; }
    }
}
