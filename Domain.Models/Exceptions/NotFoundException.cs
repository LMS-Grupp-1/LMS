namespace Domain.Models.Exceptions;

public abstract class NotFoundException : Exception
{
    public string Title { get; }
    protected NotFoundException(string message, string title = "Not Found") : base(message)
    {
        Title = title;
    }
}

