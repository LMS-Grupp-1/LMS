using LMS.Shared.DTOs.Users;
using Microsoft.AspNetCore.Identity;

namespace Service.Contracts;

public interface IUsersService
{
    Task<(bool Succeeded, IEnumerable<string> Errors)> CreateUserAsync(CreateUserDto dto);
    Task<(bool Succeeded, IEnumerable<string> Errors)> UpdateUserAsync(UpdateUserDto dto);
    Task<IEnumerable<UserDto>?> GetUsersAsync();
    Task<IEnumerable<UserDto>?> GetStudentsByCourseIdAsync(int courseId);
    Task<UserDto?> GetUserByIdAsync(string id);
}
