using Domain.Models.Entities;

namespace Domain.Contracts;

public interface INotificationRepository : IBaseRepository<Notification>
{
    Task<IReadOnlyList<Notification>> GetNotificationsForUserAsync(string userId, bool trackChanges);
    Task<int> GetUnreadCountForUserAsync(string userId);
    Task<Notification?> GetByIdAndUserAsync(int notificationId, string userId, bool trackChanges);
    void CreateNotifications(IEnumerable<Notification> notifications);
}
