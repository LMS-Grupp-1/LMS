using LMS.Blazor.Client.Services.ApiProxy;
using LMS.Shared.DTOs.ModulesDtos;
using LMS.Shared.DTOs.Users;
using System.Net.Http.Json;

namespace LMS.Blazor.Client.Services;

public class ModuleService : IModuleService
{
    private readonly IApiProxyClient _apiProxy;

    public ModuleService(IApiProxyClient apiProxy)
    {
        _apiProxy = apiProxy;
    }

    public async Task<IEnumerable<ModuleDto>> GetModulesByCourseIdAsync(int courseId)
    {
        try
        {
            var result = await _apiProxy.SendAsync<IEnumerable<ModuleDto>>(
                HttpMethod.Get,
                $"teacher/courses/{courseId}/modules"
            );
            return result ?? new List<ModuleDto>();
        }
        catch (Exception)
        {
            return new List<ModuleDto>();
        }
    }

    public async Task<bool> CreateModuleAsync(int courseId, CreateModuleDto dto)
    {
        using var content = JsonContent.Create(dto);
        return await _apiProxy.SendAsync<bool>(HttpMethod.Post, $"teacher/courses/{courseId}/modules", content);
    }

    public async Task<bool> UpdateModuleAsync(int courseId, int moduleId, CreateModuleDto dto)
    {
        using var content = JsonContent.Create(dto);
        return await _apiProxy.SendAsync<bool>(HttpMethod.Put, $"teacher/courses/{courseId}/modules/{moduleId}", content);
    }

    public async Task<bool> DeleteModuleAsync(int courseId, int moduleId)
    {
        return await _apiProxy.SendAsync<bool>(HttpMethod.Delete, $"teacher/courses/{courseId}/modules/{moduleId}");
    }
}