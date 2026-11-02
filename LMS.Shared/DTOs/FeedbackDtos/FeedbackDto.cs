namespace LMS.Shared.DTOs.FeedbackDtos;

public sealed class FeedbackDto
{
    public int Id { get; set; }
    public int SubmissionId { get; set; }
    public string TeacherId { get; set; } = string.Empty;
    public string TeacherName { get; set; } = string.Empty; // Mapped from ApplicationUser
    public string Comment { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; }
}