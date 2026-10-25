using Domain.Contracts;
using Domain.Models.Entities;
using LMS.Infrastructure.Data;
using LMS.Shared.DTOs.DashboardDtos;
using LMS.Shared.DTOs.CourseDtos;
using Microsoft.EntityFrameworkCore;

namespace LMS.Infrastructure.Repositories;

public class ActivityRepository(ApplicationDbContext context)
	: BaseRepository<Activity>(context), IActivityRepository
{
	public async Task<IEnumerable<Activity>> GetActivityByModuleIdAsync(int moduleId)
	{
		return await _context.Activities
			.Include(activity => activity.ActivityType)
			.Where(activity => activity.ModuleId == moduleId)
			.OrderBy(activity => activity.StartDate)
			.ToListAsync();
	}

    public async Task<IReadOnlyList<TeacherDashboardActivityDto>> GetUpcomingActivitiesAsync(int? count = null)
    {
        var today = DateTimeOffset.UtcNow;

        IQueryable<Activity> query = _context.Activities
            .Where(a => a.StartDate >= today)
            .OrderBy(a => a.StartDate);

        if (count.HasValue && count.Value > 0)
        {
            query = query.Take(count.Value);
        }

        return await query
            .Select(a => new TeacherDashboardActivityDto(
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

    public async Task<IReadOnlyList<ActivityOverviewDto>> GetLatestActivitiesByCourseIdAsync(int courseId, int? count = null)
    {
        IQueryable<Activity> query = _context.Activities
            .Where(a => a.Module.CourseId == courseId)
            .OrderByDescending(a => a.StartDate);
        if (count.HasValue && count.Value > 0)
        {
            query = query.Take(count.Value);
        }
        return await query
            .Select(a => new ActivityOverviewDto(
                a.ActivityTypeId,
                a.Name,
                a.Module.Name,
                a.StartDate,
                a.EndDate
            ))
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
            .AsNoTracking()
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
}