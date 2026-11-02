namespace Domain.Models.Exceptions;

public sealed class EntityNotFoundException : NotFoundException
{
    public EntityNotFoundException(string entityName, object entityId)
        : base($"{entityName} with ID {entityId} not found.", "Not Found")
    { }
}