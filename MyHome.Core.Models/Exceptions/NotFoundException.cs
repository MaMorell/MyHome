using MyHome.Core.Models.Entities;

namespace MyHome.Core.Models.Exceptions;

public class NotFoundException : Exception
{
    public NotFoundException()
    {
    }

    public NotFoundException(string? message) : base(message)
    {
    }

    public NotFoundException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}

public class NotFoundException<T>(Guid id) : NotFoundException($"Entity of type '{nameof(T)}' with ID '{id}' not found.") where T : IEntity
{
}
