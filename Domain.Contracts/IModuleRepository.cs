using Domain.Models.Entities;
using LMS.Shared.DTOs.StatisticsDtos;
using System;
using Module = Domain.Models.Entities.Module;

namespace Domain.Contracts;

public interface IModuleRepository : IBaseRepository<Module>
{
    Task<IEnumerable<Module>> GetModulesByCourseIdAsync(int courseId);
    Task<ModuleStatisticsDto?> GetModuleStatisticsAsync(int moduleId, CancellationToken cancellationToken = default);
}