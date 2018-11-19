using MongoDB.Bson;
using System.Collections.Generic;

namespace Abyss.Web.Entities
{
    public class Role : BaseEntity
    {
        public string Name { get; set; }
        public List<ObjectId> Permissions { get; set; }
    }
}