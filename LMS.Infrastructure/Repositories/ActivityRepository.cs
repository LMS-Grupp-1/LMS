using Domain.Contracts;
using Domain.Models.Entities;
using LMS.Infrastructure.Data;
using LMS.Shared.DTOs.DashboardDtos;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens.Experimental;

namespace LMS.Infrastructure.Repositories;

public class ActivityRepository(ApplicationDbContext context) : IActivityRepository
{
    private readonly ApplicationDbContext _context = context;
    
    public async Task<IEnumerable<Activity>> GetActivityByModuleIdAsync(int moduleId)
    {
        return await _context.Activities
			.Include(m => m.ActivityType)
			.Where(m => m.ModuleId == moduleId)
            .OrderBy(m => m.StartDate)
            .ToListAsync();
    }

	public async Task<IReadOnlyList<DashboardActivityDto>> GetUpcomingByCourseIdAsync(
		int courseId,
		DateTime now,
		int take,
		CancellationToken cancellationToken = default)
	{
		return await _context.Activities
			.AsNoTracking()
			.Where(activity => activity.Module.CourseId == courseId && activity.EndDate >= now)
			.OrderBy(activity => activity.StartDate)
			.Take(take)
			.Select(activity => new DashboardActivityDto(
				activity.Id,
				activity.Name,
				activity.ActivityType.Name,
				activity.StartDate,
				activity.EndDate,
				activity.ModuleId,
				activity.Module.Name))
			.ToListAsync(cancellationToken);
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