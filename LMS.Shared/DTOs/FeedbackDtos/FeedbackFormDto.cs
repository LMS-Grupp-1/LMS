namespace LMS.Shared.DTOs.FeedbackDtos;

public sealed class FeedbackFormDto
{
    public int SubmissionId { get; set; }
    public string Comment { get; set; } = string.Empty;
}