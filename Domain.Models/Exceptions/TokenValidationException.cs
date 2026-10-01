namespace Domain.Models.Exceptions;

public class TokenValidationException : Exception
{
    public TokenValidationException(string message = "Invalid or expired token") : base(message)
    {

    }
}
