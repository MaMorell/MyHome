using MyHome.Core.Models.Entities;

namespace MyHome.Core.Interfaces;

public interface IRepository<T> where T : IEntity
{
    Task<T> GetByIdAsync(Guid id);
    Task<IEnumerable<T>> GetAllAsync();
    Task UpsertAsync(T entity);
}
