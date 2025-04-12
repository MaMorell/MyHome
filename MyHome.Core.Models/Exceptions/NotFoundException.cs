using MyHome.Core.Models.Entities;

namespace MyHome.Core.Models.Exceptions;

public class NotFoundException<T>(Guid id) : Exception($"Entity of type '{nameof(T)}' with ID '{id}' not found.") where T : IEntity
{
}
