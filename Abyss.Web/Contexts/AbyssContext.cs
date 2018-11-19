using Abyss.Web.Contexts.Interfaces;
using Abyss.Web.Entities;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;

namespace Abyss.Web.Contexts
{
    public class AbyssContext : IAbyssContext
    {
        private readonly IMongoDatabase _database;
        public AbyssContext(IMongoDatabase database)
        {
            _database = database;
        }

        public IMongoCollection<T> GetCollection<T>()
        {
            return _database.GetCollection<T>(typeof(T).Name);
        }
    }
}
