using Domain.Contracts;
using Domain.Models.Entities;
using LMS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LMS.Infrastructure.Repositories;

public class NotificationRepository(ApplicationDbContext context) : BaseRepository<Notification>(context), INotificationRepository
{
    public async Task<IReadOnlyList<Notification>> GetNotificationsForUserAsync(string userId, bool trackChanges)
    {
        var query = Set
            .Where(n => n.UserId == userId && !n.IsRead)
            .OrderByDescending(n => n.CreatedAt);

        return trackChanges
            ? await query.ToListAsync()
            : await query.AsNoTracking().ToListAsync();
    }

    public async Task<int> GetUnreadCountForUserAsync(string userId)
    {
        return await Set
            .AsNoTracking()
            .CountAsync(n => n.UserId == userId && !n.IsRead);
    }

    public async Task<Notification?> GetByIdAndUserAsync(int notificationId, string userId, bool trackChanges)
    {
        var query = Set
            .Where(n => n.Id == notificationId && n.UserId == userId);

        return trackChanges
            ? await query.FirstOrDefaultAsync()
            : await query.AsNoTracking().FirstOrDefaultAsync();
    }

    public void CreateNotifications(IEnumerable<Notification> notifications) =>
        Set.AddRange(notifications);
}