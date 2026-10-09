using Domain.Models.Entities;
using System;
using Module = Domain.Models.Entities.Module;

namespace Domain.Contracts;

public interface IModuleRepository
{
    Task<IEnumerable<Module>> GetModulesByCourseIdAsync(int courseId);
    Task<Module?> GetByIdAsync(int id);
    Task AddAsync(Module module);
    void Update(Module module);
    void Delete(Module module);
}