namespace Domain.Models.Entities;

public class Notification
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty; // Owner of the notification
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public bool IsRead { get; set; } = false;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public string NotificationType { get; set; } = string.Empty;

    public int TargetId;

    // Navigation property
    public ApplicationUser User { get; set; } = null!;
}