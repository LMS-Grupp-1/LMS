using LMS.Shared.DTOs.ModulesDtos;

namespace LMS.Blazor.Client.Services;

public interface IModuleService
{
    Task<IEnumerable<ModuleDto>> GetModulesByCourseIdAsync(int courseId);
    Task<bool> CreateModuleAsync(int courseId, CreateModuleDto dto);
}
