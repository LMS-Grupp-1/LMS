using LMS.Shared.DTOs.Users;

namespace Domain.Contracts;

public interface IUserRepository
{
    Task<IEnumerable<UserDto>> GetUsersWithCoursesAsync();
    Task<IEnumerable<UserDto>> GetStudentsByCourseIdAsync(int courseId);
}