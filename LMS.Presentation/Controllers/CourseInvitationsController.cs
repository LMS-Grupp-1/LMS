using LMS.Shared.Constants;
using LMS.Shared.DTOs.CourseInvitationDtos;
using LMS.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service.Contracts;

namespace LMS.Presentation.Controllers;

[Authorize]
[ApiController]
[Route("api/course-invitations")]
public class CourseInvitationsController : ControllerBase
{
    private readonly IServiceManager _service;
    public CourseInvitationsController(IServiceManager service)
    {
        _service = service;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] InvitedStudentRegisterDto dto, CancellationToken cancellationToken)
    {
        await _service.CourseInvitationService.RegisterInvitedStudentAsync(dto, cancellationToken);

        return Ok(true);
    }
}
