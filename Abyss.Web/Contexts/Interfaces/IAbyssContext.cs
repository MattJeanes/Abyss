using Abyss.Web.Entities;
using MongoDB.Driver;

namespace Abyss.Web.Contexts.Interfaces
{
    public interface IAbyssContext
    {
        IMongoCollection<T> GetCollection<T>();
    }
}
