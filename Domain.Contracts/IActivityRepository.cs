

using Domain.Models.Entities;

namespace Domain.Contracts;

public interface IActivityRepository : IBaseRepository<Activity>
{
    Task<IEnumerable<Activity>> GetActivityByModuleIdAsync(int moduleId);
}