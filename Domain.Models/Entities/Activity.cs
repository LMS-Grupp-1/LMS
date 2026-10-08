namespace Domain.Models.Entities;

public class Activity
{
    public int Id { get; set; }
    public int ModuleId { get; set; }
    public int ActivityTypeId {get; set; }
    public string Name { get; set; } = string.Empty;
    string Description { get; set; } = string.Empty;
    DateTime StartDate { get; set; }
    DateTime EndDate { get; set; }
}