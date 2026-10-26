using LMS.Shared.DTOs.ModulesDtos;

namespace Service.Contracts;

public interface IModuleService
{
    Task<IEnumerable<ModuleDto>> GetModulesByCourseIdAsync(int courseId);
    Task CreateModuleAsync(int courseId, CreateModuleDto dto);
    Task UpdateModuleAsync(int courseId, int moduleId, CreateModuleDto dto);
    Task DeleteModuleAsync(int courseId, int moduleId);
}
