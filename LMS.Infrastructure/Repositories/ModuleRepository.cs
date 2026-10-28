
using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Domain.Contracts;
using Domain.Models.Entities;
using LMS.Infrastructure.Data;
using Module = Domain.Models.Entities.Module;

namespace LMS.Infrastructure.Repositories;

public class ModuleRepository : BaseRepository<Module>, IModuleRepository
{
    public ModuleRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Module>> GetModulesByCourseIdAsync(int courseId)
    {
        return await Set
            .Where(m => m.CourseId == courseId)
            .OrderBy(m => m.StartDate)
            .ToListAsync();
    }
}