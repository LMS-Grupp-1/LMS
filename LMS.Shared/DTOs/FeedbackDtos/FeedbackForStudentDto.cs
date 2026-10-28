namespace LMS.Shared.DTOs.FeedbackDtos;

public record FeedbackForStudentDto(
    int Id,
    string TeacherName,
    string Comment,
    DateTimeOffset CreatedAt
);