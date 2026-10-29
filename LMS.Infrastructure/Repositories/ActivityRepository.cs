using Domain.Contracts;
using Domain.Models.Entities;
using LMS.Shared.DTOs.ActivityDtos;
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
		return await Set
			.Include(activity => activity.ActivityType)
			.Where(activity => activity.ModuleId == moduleId)
			.OrderBy(activity => activity.StartDate)
			.ToListAsync();
	}

    public async Task<ActivityDeletionPreviewDto?> GetDeletionPreviewAsync(
        int moduleId,
        int activityId)
    {
        return await Set
            .AsNoTracking()
            .Where(activity => activity.Id == activityId && activity.ModuleId == moduleId)
            .Select(activity => new ActivityDeletionPreviewDto(
                activity.Id,
                activity.Name,
                activity.Documents
                    .OrderBy(document => document.Name)
                    .ThenBy(document => document.Id)
                    .Select(document => new ActivityDeletionDocumentDto(
                        document.Id,
                        document.Name,
                        document.FileName))
                    .ToList(),
                activity.Assignments
                    .OrderBy(assignment => assignment.Name)
                    .ThenBy(assigment => assigment.Id)
                    .Select(assignment => new ActivityDeletionAssignmentDto(
                        assignment.Id,
                        assignment.Name,
                        assignment.Submissions.Count(),
                        assignment.Submissions
                            .SelectMany(submission => submission.Feedbacks)
                            .Count()))
                    .ToList()))
            .SingleOrDefaultAsync();
    }

	public async Task<Activity?> GetWithModuleAndCourseAsync(int id)
	{
		return await _context.Activities
			.AsNoTracking()
			.Include(activity => activity.Module)
				.ThenInclude(module => module.Course)
			.FirstOrDefaultAsync(activity => activity.Id == id);
	}

	public async Task<IReadOnlyList<TeacherDashboardActivityDto>> GetUpcomingActivitiesAsync(int? count = null)
    {
        var today = DateTimeOffset.UtcNow;

        IQueryable<Activity> query = Set
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
        IQueryable<Activity> query = Set
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
        return await Set
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
        var query = Set
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