using System.Net.Http.Json;
using LMS.Blazor.Client.Services.ApiProxy;
using LMS.Shared.DTOs.Users;

namespace LMS.Blazor.Client.Services;

public class UsersService : IUsersService
{
    private readonly IApiProxyClient _apiProxy;

    public UsersService(IApiProxyClient apiProxy)
    {
        _apiProxy = apiProxy;
    }

    public async Task<bool> CreateUserAsync(CreateUserDto dto)
    {
        using var content = JsonContent.Create(dto);
        return await _apiProxy.SendAsync<bool>(HttpMethod.Post, "users", content);
    }

    public async Task<IEnumerable<UserDto>?> GetUsersAsync()
    {
        return await _apiProxy.SendAsync<IEnumerable<UserDto>>(HttpMethod.Get, "users");
    }

    public async Task<UserDto?> GetUserByIdAsync(string id) =>
    await _apiProxy.SendAsync<UserDto>(HttpMethod.Get, $"users/{id}");

    public async Task<bool> UpdateUserAsync(UpdateUserDto dto)
    {
        using var content = JsonContent.Create(dto);
        return await _apiProxy.SendAsync<bool>(HttpMethod.Put, $"users/{dto.Id}", content);
    }
}