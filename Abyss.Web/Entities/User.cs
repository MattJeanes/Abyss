using Abyss.Web.Data;
using MongoDB.Bson.Serialization.Attributes;
using System.Collections.Generic;

namespace Abyss.Web.Entities
{
    public class User : BaseEntity
    {
        public string Name { get; set; }
        public Dictionary<string, string> Authentication { get; set; }
    }
}
