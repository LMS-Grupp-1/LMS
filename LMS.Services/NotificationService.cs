using Domain.Contracts;
using Domain.Models.Exceptions;
using LMS.Shared.DTOs.NotificationDtos;
using Service.Contracts;
using System;
using System.Collections.Generic;
using System.Text;

namespace LMS.Services;

public class NotificationService : INotificationService
{
    private readonly IUnitOfWork _uow;

    public NotificationService(IUnitOfWork uow) => _uow = uow;

    public async Task<IReadOnlyList<NotificationDto>> GetMyNotificationsAsync(string userId)
    {
        var notifications = await _uow.Notifications.GetNotificationsForUserAsync(userId, trackChanges: false);

        return notifications.Select(n => new NotificationDto(
            n.Id,
            n.Title,
            n.Message,
            n.IsRead,
            n.CreatedAt,
            n.NotificationType,
            n.TargetId
        )).ToList();
    }

    public async Task<int> GetUnreadCountAsync(string userId) =>
        await _uow.Notifications.GetUnreadCountForUserAsync(userId);

    public async Task MarkAsReadAsync(int notificationId, string userId)
    {
        var notification = await _uow.Notifications.GetByIdAndUserAsync(notificationId, userId, trackChanges: true);
        if (notification is null)
            throw new EntityNotFoundException("Notification", notificationId);

        notification.IsRead = true;

        await _uow.CompleteAsync();
    }

    public async Task MarkAllAsReadAsync(string userId)
    {
        var notifications = await _uow.Notifications.GetNotificationsForUserAsync(userId, trackChanges: true);
        foreach (var n in notifications.Where(x => !x.IsRead))
        {
            n.IsRead = true;
        }

        await _uow.CompleteAsync();
    }
}