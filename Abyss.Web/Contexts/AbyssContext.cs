using Abyss.Web.Contexts.Interfaces;
using MongoDB.Driver;

namespace Abyss.Web.Contexts;

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
