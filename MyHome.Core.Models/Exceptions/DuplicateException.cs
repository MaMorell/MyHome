namespace MyHome.Core.Models.Exceptions;

public class DuplicateException(Guid id) : Exception($"Entity with ID {id} already exists.")
{
}
