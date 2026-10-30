namespace Domain.Models.Exceptions;

public class ForbiddenException : Exception
{
    public ForbiddenException() : base("You don't have permission to access this resource.")
    {
    }

    public ForbiddenException(string message) : base(message)
    {
    }

    protected ForbiddenException(string message, Exception innerException) : base(message, innerException)
    {
    }
}