using LMS.Shared.DTOs.Users;

namespace LMS.Blazor.Client.Services;

public interface IUsersService
{
    Task<bool> CreateUserAsync(CreateUserDto dto);
    Task<IEnumerable<UserDto>?> GetUsersAsync();
    Task<UserDto?> GetUserByIdAsync(string id);
    Task<bool> UpdateUserAsync(UpdateUserDto dto);
}