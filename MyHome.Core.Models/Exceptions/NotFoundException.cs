namespace MyHome.Core.Models.Exceptions;

public class NotFoundException(Guid id) : Exception($"Entity with ID {id} not found.")
{
}
