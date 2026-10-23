using Domain.Models.Entities;
using LMS.Shared.DTOs;

namespace Domain.Contracts;

public interface IActivityRepository : IBaseRepository<Activity>
{
    Task<IEnumerable<Activity>> GetActivityByModuleIdAsync(int moduleId);

    Task<IEnumerable<DashboardActivityDto>> GetDateRangeActivitiesAsync(DateTime startDate, DateTime endDate, int? count = null);

    Task<bool> HasOverlapAsync(
        int moduleId,
        DateTime startDate,
        DateTime endDate,
        int? excludedActivityId = null,
        CancellationToken cancellationToken = default);
}   