namespace LMS.Blazor.Client.ViewModels;

public class StudentViewModel
{
    public string Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
}