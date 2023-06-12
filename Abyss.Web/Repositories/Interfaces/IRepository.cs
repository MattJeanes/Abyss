namespace Abyss.Web.Repositories.Interfaces;

public interface IRepository<T>
{
    IQueryable<T> GetAll();
    Task<T> GetById(int id);
    void Add(T item);
    void Remove(T item);
    Task SaveChanges();
}
