namespace WebApp.Repositories;

public interface IRepository<T, TKey> where T : class
{
    Task<T?> GetByIdAsync(TKey id);
    IQueryable<T> GetAll();
    Task AddAsync(T entity);
    void UpdateAsync(T entity);
    Task SaveChangesAsync();
}