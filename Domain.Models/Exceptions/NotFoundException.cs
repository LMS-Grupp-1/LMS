namespace Domain.Models.Exceptions;

public abstract class NotFoundException : Exception
{
    public string Title { get; }
    public string Code { get; }
    protected NotFoundException(string message, string title = "Not Found", string code = "NotFound") : base(message)
    {
        Title = title;
        Code = code;
    }
}
