using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Abyss.Web.Data.Options
{
    public class CloudflareOptions
    {
        public string BaseUrl { get; set; }
        public string Email { get; set; }
        public string ApiKey { get; set; }
        public string Zone { get; set; }
    }
}
