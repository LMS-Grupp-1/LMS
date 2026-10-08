using LMS.Shared.DTOs.Users;
using Microsoft.AspNetCore.Identity;

namespace Service.Contracts;

public interface IUsersService
{
    Task<IdentityResult> CreateUserAsync(CreateUserDto dto);
    Task<IEnumerable<UserDto>?> GetUsersAsync();

    Task<IdentityResult> UpdateUserAsync(UpdateUserDto dto);
    Task<UserDto?> GetUserByIdAsync(string id);
}
