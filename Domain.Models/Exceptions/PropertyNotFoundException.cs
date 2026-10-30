namespace Domain.Models.Exceptions;

public sealed class PropertyNotFoundException : NotFoundException
{
    public PropertyNotFoundException(string message)
        : base(message, "Not Found")
    { }
}