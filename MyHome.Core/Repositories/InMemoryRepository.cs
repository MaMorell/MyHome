using MyHome.Core.Models;
using MyHome.Core.Models.Exceptions;

namespace MyHome.Core.Repositories;

public class InMemoryRepository<T> : IRepository<T> where T : IEntity
{
    private readonly Dictionary<Guid, T> _data = [];
    public Task<T?> GetByIdAsync(Guid id)
    {
        var result = _data.TryGetValue(id, out var entity)
            ? entity
            : default;

        return Task.FromResult(result);
    }

    public Task<IEnumerable<T>> GetAllAsync()
    {
        return Task.FromResult(_data.Values.AsEnumerable());
    }

    public Task AddAsync(T entity)
    {
        if (!_data.TryAdd(entity.Id, entity))
        {
            throw new DuplicateException(entity.Id);
        }

        return Task.CompletedTask;
    }

    public Task UpdateAsync(T entity)
    {
        if (!_data.TryGetValue(entity.Id, out var _))
        {
            throw new NotFoundException(entity.Id);
        }

        _data[entity.Id] = entity;

        return Task.CompletedTask;
    }

    public Task DeleteAsync(Guid id)
    {
        throw new NotImplementedException();
    }

    public Task UpsertAsync(T entity)
    {
        if (!_data.TryAdd(entity.Id, entity))
        {
            return UpdateAsync(entity);
        }

        return Task.CompletedTask;
    }
}