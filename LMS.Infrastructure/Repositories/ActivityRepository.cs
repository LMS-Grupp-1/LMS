using Domain.Contracts;
using Domain.Models.Entities;
using LMS.Infrastructure.Data;
using LMS.Shared.DTOs;
using Microsoft.EntityFrameworkCore;

namespace LMS.Infrastructure.Repositories;

public class ActivityRepository(ApplicationDbContext context) : IActivityRepository
{
    private readonly ApplicationDbContext _context = context;
    
    public async Task<IEnumerable<Activity>> GetActivityByModuleIdAsync(int moduleId)
    {
        return await _context.Activities
            .Where(m => m.ModuleId == moduleId)
            .OrderBy(m => m.StartDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<DashboardActivityDto>> GetDateRangeActivitiesAsync(DateTime startDate, DateTime endDate, int? count = null)
    {
        IQueryable<Activity> query = _context.Activities
            .Where(a => a.StartDate >= startDate && a.StartDate <= endDate)
            .OrderBy(a => a.StartDate);

        if (count.HasValue && count.Value > 0)
        {
            query = query.Take(count.Value);
        }

        return await query
            .Select(a => new DashboardActivityDto(
                a.Module.CourseId,
                a.ActivityTypeId,
                a.Name,
                a.Module.Course.Name,
                a.Module.Name,
                a.StartDate,
                a.EndDate
            ))
            .ToListAsync();
    }

    public async Task<bool> HasOverlapAsync(
        int moduleId,
        DateTime startDate,
        DateTime endDate,
        int? excludedActivityId = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Activities
            .Where(activity => activity.ModuleId == moduleId);

        if (excludedActivityId.HasValue)
        {
            int excludedId = excludedActivityId.Value;
            query = query.Where(activity => activity.Id != excludedId);
        }
        return await query.AnyAsync(
            activity =>
                activity.StartDate < endDate &&
                activity.EndDate > startDate,
            cancellationToken);
    }

    public async Task<Activity?> GetByIdAsync(int id)
    {
        return await _context.Activities.FindAsync(id);
    }

    public async Task AddAsync(Activity activity)
    {
        await _context.AddAsync(activity);
    }

    public void Update(Activity activity)
    {
        _context.Activities.Update(activity);
    }

    public void Delete(Activity activity)
    {
        _context.Activities.Remove(activity);
    }
}