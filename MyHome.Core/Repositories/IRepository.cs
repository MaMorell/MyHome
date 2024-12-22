using MyHome.Core.Models;

namespace MyHome.Core.Repositories;

public interface IRepository<T> where T : IEntity
{
    Task<T?> GetByIdAsync(Guid id);
    Task<IEnumerable<T>> GetAllAsync();
    Task UpsertAsync(T entity);
}
