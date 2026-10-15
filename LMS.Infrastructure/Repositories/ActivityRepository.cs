using Domain.Models.Entities;
using Domain.Contracts;
using LMS.Infrastructure.Data;
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