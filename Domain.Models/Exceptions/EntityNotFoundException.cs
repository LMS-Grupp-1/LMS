namespace Domain.Models.Exceptions;

public sealed class EntityNotFoundException : NotFoundException
{
    public EntityNotFoundException(string entityName, object entityId)
        : base($"{entityName} med ID {entityId} hittades inte.", "Not Found")
    { }
}