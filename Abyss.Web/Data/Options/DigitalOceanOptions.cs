using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Abyss.Web.Data.Options
{
    public class DigitalOceanOptions
    {
        public string ApiKey { get; set; }
        public int ActionTimeout { get; set; }
        public int TimeBetweenChecks { get; set; }
        public int SshId { get; set; }
    }
}
