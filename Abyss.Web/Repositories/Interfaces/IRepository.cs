using MongoDB.Bson;
using MongoDB.Driver.Linq;
using System.Threading.Tasks;

namespace Abyss.Web.Repositories.Interfaces
{
    public interface IRepository<T>
    {
        IMongoQueryable<T> GetAll();
        Task<T> GetById(ObjectId id);
        Task<T> GetById(string id);
        Task AddOrUpdate(T item);
        Task Add(T item);
        Task Update(T item);
        Task Remove(T item);
        Task Remove(ObjectId id);
    }
}
