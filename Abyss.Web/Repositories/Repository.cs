using Abyss.Web.Contexts;
using Abyss.Web.Entities;
using Abyss.Web.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Abyss.Web.Repositories;

public class Repository<T> : IRepository<T> where T : BaseEntity
{
    private readonly AbyssContext _context;
    protected readonly DbSet<T> _repository;

    public Repository(AbyssContext context)
    {
        _context = context;
        _repository = _context.Set<T>();
    }

    public virtual IQueryable<T> GetAll()
    {
        return _repository.AsQueryable();
    }

    public virtual async Task<T> GetById(int id)
    {
        var item = await GetAll().Where(x => x.Id == id).FirstOrDefaultAsync();
        return item;
    }

    public void Add(T item)
    {
        _repository.Add(item);
    }

    public void Remove(T item)
    {
        _repository.Remove(item);
    }

    public async Task SaveChanges()
    {
        await _context.SaveChangesAsync();
    }
}
