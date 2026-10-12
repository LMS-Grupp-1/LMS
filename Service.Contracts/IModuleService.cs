using LMS.Shared.DTOs.ModulesDtos;

namespace Service.Contracts;

public interface IModuleService
{
    Task<IEnumerable<ModuleDto>> GetModulesByCourseIdAsync(int courseId);
    Task<(bool Success, string ErrorMessage)> CreateModuleAsync(int courseId, CreateModuleDto dto);
}
