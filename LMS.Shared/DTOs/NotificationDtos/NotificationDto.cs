using System;
using System.Collections.Generic;
using System.Text;

namespace LMS.Shared.DTOs.NotificationDtos;

public record NotificationDto(
    int Id,
    string Title,
    string Message,
    bool IsRead,
    DateTimeOffset CreatedAt,
    string NotificationType,
    int TargetId
);

public record MarkNotificationAsReadDto(int NotificationId);
