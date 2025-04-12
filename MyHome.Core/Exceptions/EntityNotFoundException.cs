namespace MyHome.Core.Exceptions;

public class EntityNotFoundException : Exception
{
    public EntityNotFoundException()
    {
    }

    public EntityNotFoundException(string? message) : base(message)
    {
    }

    public EntityNotFoundException(Guid id) : base($"Entity with ID {id} not found")
    {
    }
}
