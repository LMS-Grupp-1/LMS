using Domain.Models.Entities;

namespace Domain.Contracts;

public interface ICourseInvitationRepository : IBaseRepository<CourseInvitation>
{
    Task<CourseInvitation?> GetByEmailAndTokenAsync(string email, string token, CancellationToken cancellationToken = default);
}
