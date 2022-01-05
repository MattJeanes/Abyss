using Abyss.Web.Contexts.Interfaces;
using Abyss.Web.Entities;
using Abyss.Web.Repositories.Interfaces;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace Abyss.Web.Repositories;

public class Repository<T> : IRepository<T> where T : BaseEntity
{
    private readonly IAbyssContext _context;
    protected readonly IMongoCollection<T> _repository;

    public Repository(IAbyssContext context)
    {
        _context = context;
        _repository = _context.GetCollection<T>();
    }

    public IMongoQueryable<T> GetAll()
    {
        return _repository.AsQueryable();
    }

    public async Task<T> GetById(ObjectId id)
    {
        var item = await GetAll().Where(x => x.Id == id).FirstOrDefaultAsync();
        return item;
    }

    public async Task<T> GetById(string id)
    {
        var item = await GetById(ObjectId.Parse(id));
        return item;
    }

    public async Task AddOrUpdate(T item)
    {
        if (item.Id == default(ObjectId))
        {
            await Add(item);
        }
        else
        {
            await Update(item);
        }
    }

    public async Task Add(T item)
    {
        await _repository.InsertOneAsync(item);
    }

    public async Task Update(T item)
    {
        var filter = Builders<T>.Filter.Eq(s => s.Id, item.Id);
        await _repository.ReplaceOneAsync(filter, item);
    }

    public async Task Remove(T item)
    {
        await Remove(item.Id);
    }

    public async Task Remove(ObjectId id)
    {
        var filter = Builders<T>.Filter.Eq(s => s.Id, id);
        await _repository.DeleteOneAsync(filter);
    }
}
