
using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Domain.Contracts;
using Domain.Models.Entities;
using LMS.Infrastructure.Data;
using Module = Domain.Models.Entities.Module;

namespace LMS.Infrastructure.Repositories;

public class ModuleRepository : IModuleRepository
{
    private readonly ApplicationDbContext _context;

    public ModuleRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Module>> GetModulesByCourseIdAsync(int courseId)
    {
        return await _context.Modules
            .Where(m => m.CourseId == courseId)
            .OrderBy(m => m.StartDate)
            .ToListAsync();
    }

    public async Task<Module?> GetByIdAsync(int id)
    {
        return await _context.Modules
            .Include(m => m.Course)
            .FirstOrDefaultAsync(m => m.Id == id);
    }

    public async Task AddAsync(Module module)
    {
        await _context.Modules.AddAsync(module);
    }

    public void Update(Module module)
    {
        _context.Modules.Update(module);
    }

    public void Delete(Module module)
    {
        _context.Modules.Remove(module);
    }
}
