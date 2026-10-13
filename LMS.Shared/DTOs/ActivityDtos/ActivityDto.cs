namespace LMS.Shared.DTOs.ActivityDtos;

public class ActivityDto
{
    public int Id { get; set; }
  //  public int ModuleId { get; set; } // add Module?
    public string ActivityType {get; set; }
    public string Name { get; set; } = string.Empty;
    string Description { get; set; } = string.Empty;
    DateTime StartDate { get; set; }
    DateTime EndDate { get; set; }
}