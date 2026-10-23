using Domain.Models.Entities;
using LMS.Shared.DTOs.DashboardDtos;

namespace Domain.Contracts;

public interface IActivityRepository : IBaseRepository<Activity>
{
    Task<IEnumerable<Activity>> GetActivityByModuleIdAsync(int moduleId);

    Task<bool> HasOverlapAsync(
        int moduleId,
        DateTime startDate,
        DateTime endDate,
        int? excludedActivityId = null,
        CancellationToken cancellationToken = default);

	Task<IReadOnlyList<DashboardActivityDto>> GetUpcomingByCourseIdAsync(
		int courseId,
		DateTime now,
		int take,
		CancellationToken cancellationToken = default);
}   