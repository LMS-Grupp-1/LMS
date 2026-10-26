namespace Domain.Models.Entities;

public class Activity
{
    public int Id { get; set; }
	public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }

    // Foreign keys
    public int ModuleId { get; set; }
    public int ActivityTypeId { get; set; }

    // Navigation properties
    public Module Module { get; set; } = null!;
    public ActivityType ActivityType { get; set; } = null!;

    public ICollection<Document> Documents { get; set; } = new List<Document>();
}