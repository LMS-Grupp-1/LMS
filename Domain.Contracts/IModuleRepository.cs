using Domain.Models.Entities;
using System;
using Module = Domain.Models.Entities.Module;

namespace Domain.Contracts;

public interface IModuleRepository : IBaseRepository<Module>
{
    Task<IEnumerable<Module>> GetModulesByCourseIdAsync(int courseId);
}