using LMS.Shared.DTOs.Users;

namespace LMS.Blazor.Client.Services;

public interface IUsersService
{
    Task<HttpResponseMessage> CreateUserAsync(CreateUserDto dto);
}