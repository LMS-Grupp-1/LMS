using LMS.Shared.DTOs.ModulesDtos;

namespace Service.Contracts;

public interface IModuleService
{
    Task<IEnumerable<ModuleDto>> GetModulesByCourseIdAsync(int courseId);
    Task<(bool Success, string ErrorMessage)> CreateModuleAsync(int courseId, CreateModuleDto dto);
    Task<(bool Success, string ErrorMessage)> UpdateModuleAsync(int courseId, int moduleId, CreateModuleDto dto);
    Task<(bool Success, string ErrorMessage)> DeleteModuleAsync(int courseId, int moduleId);
}
