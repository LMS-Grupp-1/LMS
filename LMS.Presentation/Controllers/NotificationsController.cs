

using LMS.Shared.DTOs.NotificationDtos;
using LMS.Shared.Constants;
using Service.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LMS.Presentation.Controllers;

[Authorize]
[ApiController]
[Route("api/notifications")]
public class NotificationsController : ControllerBase
{
    private readonly IServiceManager _service;

    public NotificationsController(IServiceManager service)
    {
        _service = service;
    }

    private string GetCurrentUserId() =>
        User.FindFirstValue(ClaimTypes.NameIdentifier) ?? throw new UnauthorizedAccessException("User is not authenticated.");

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<NotificationDto>>> GetMyNotifications()
    {
        var userId = GetCurrentUserId();

        var notifications = await _service.NotificationService.GetMyNotificationsAsync(userId);
        return Ok(notifications);
    }

    [HttpPost("{id}/read")]
    public async Task<IActionResult> MarkAsRead(int id)
    {
        var userId = GetCurrentUserId();

        await _service.NotificationService.MarkAsReadAsync(id, userId);
        return Ok(true);
    }

    [HttpPost("read-all")]
    public async Task<IActionResult> MarkAllAsReadAsync()
    {
        var userId = GetCurrentUserId();
        await _service.NotificationService.MarkAllAsReadAsync(userId);
        return Ok(true);
    }
}
