

namespace Service.Contracts;

public interface ICourseInvitationService
{
    Task InviteStudentAsync(string email, int courseId, CancellationToken cancellationToken = default);
    Task RegisterInvitedStudentAsync(InvitedStudentRegisterDto dto, CancellationToken cancellationToken = default);
}
