using Domain.Models.Entities;
using Domain.Contracts;
using LMS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens.Experimental;

namespace LMS.Infrastructure.Repositories;

public class ActivityRepository(ApplicationDbContext context) 
    : BaseRepository<Activity>(context), IActivityRepository
{
    public async Task<IEnumerable<Activity>> GetActivityByModuleIdAsync(int moduleId)
    {
        return await _context.Activities
            .Where(activity => activity.ModuleId == moduleId)
            .OrderBy(activity => activity.StartDate)
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
}