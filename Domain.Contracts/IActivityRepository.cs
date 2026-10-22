using Domain.Models.Entities;

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
}   