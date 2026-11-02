

namespace LMS.Shared.DTOs.CourseInvitationDtos;

public class InviteStudentDto
{
    public string Email { get; set; } = string.Empty;
    public int CourseId { get; set; }
}
