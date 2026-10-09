using LMS.Shared.DTOs.ModulesDtos;
using LMS.Shared.DTOs.Users;

namespace LMS.Blazor.Client.Services;

public interface IModuleService
{
    Task<IEnumerable<ModuleDto>> GetModulesByCourseIdAsync(int courseId);
    Task<bool> CreateModuleAsync(int courseId, CreateModuleDto dto);
    Task<bool> UpdateModuleAsync(int courseId, int moduleId, CreateModuleDto dto);
    Task<bool> DeleteModuleAsync(int courseId, int moduleId);
}
