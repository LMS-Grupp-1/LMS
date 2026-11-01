
using Domain.Contracts;
using Domain.Models.Entities;
using LMS.Infrastructure.Data;
using LMS.Shared.DTOs.StatisticsDtos;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
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

    public async Task<ModuleStatisticsDto?> GetModuleStatisticsAsync(int moduleId, CancellationToken cancellationToken = default)
    {
        return await Set
            .AsNoTracking()
            .Where(m => m.Id == moduleId)
            .Select(m => new ModuleStatisticsDto(
                m.Activities.Count(),
                m.Assignments.Count(),
                m.Documents.Count()
            ))
            .FirstOrDefaultAsync(cancellationToken);
    }
}