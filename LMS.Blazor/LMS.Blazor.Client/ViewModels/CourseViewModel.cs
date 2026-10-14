namespace LMS.Blazor.Client.ViewModels;

public class CourseViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int StudentCount { get; set; } = 0;
    public int ModuleCount { get; set; } = 0;
}
