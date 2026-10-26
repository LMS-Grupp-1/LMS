namespace Domain.Models.Entities;

public class ActivityType
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;

    // Navigation property: One ActivityType can have many Activities
    public ICollection<Activity> Activities { get; set; } = new List<Activity>();
}