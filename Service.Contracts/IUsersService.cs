using LMS.Shared.DTOs.Users;
using Microsoft.AspNetCore.Identity;

namespace Service.Contracts;

public interface IUsersService
{
    Task CreateUserAsync(CreateUserDto dto);
    Task UpdateUserAsync(UpdateUserDto dto);
    Task<IEnumerable<UserDto>> GetUsersAsync();
    Task<IEnumerable<UserDto>> GetStudentsByCourseIdAsync(int courseId);
    Task<UserDto?> GetUserByIdAsync(string id);
}
