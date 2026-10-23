using Domain.Contracts;
using Domain.Models.Entities;
using LMS.Infrastructure.Data;
using LMS.Shared.DTOs.DashboardDtos;
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
}