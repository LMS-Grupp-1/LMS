using Domain.Models.Entities;
using LMS.Shared.DTOs.Users;

namespace Domain.Contracts;

public interface IUserRepository : IBaseRepository<ApplicationUser>
{
    Task<ApplicationUser?> FindByIdAsync(string id);
    Task<IEnumerable<UserDto>> GetUsersWithCoursesAsync();
    Task<IEnumerable<UserDto>> GetStudentsByCourseIdAsync(int courseId);
    Task<IReadOnlyList<UserLookupDto>> GetUsersByCourseIdAsync(int courseId, string? role = null);
}