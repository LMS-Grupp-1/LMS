using System;

namespace Domain.Models.Entities;

public class Module
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }

    // Foreign key to Course
    public int CourseId { get; set; }
    public Course Course { get; set; } = null!;

    // Navigation property for child activities
    // public ICollection<Activity> Activities { get; set; } = new List<Activity>();
}