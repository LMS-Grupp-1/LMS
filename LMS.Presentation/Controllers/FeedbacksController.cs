using Service.Contracts;
using LMS.Shared.DTOs.FeedbackDtos;
using LMS.Shared.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LMS.Presentation.Controllers;

[ApiController]
[Route("api/submissions/{submissionId:int}/[controller]")]
[Authorize]
public class FeedbacksController : ControllerBase
{
    private readonly IServiceManager _service;

    public FeedbacksController(IServiceManager service)
    {
        _service = service;
    }

    [HttpGet]
    [Authorize(Roles = UserRoles.Teacher)]
    public async Task<ActionResult<IEnumerable<FeedbackDto>>> GetFeedbacksBySubmissionId(
        int submissionId,
        CancellationToken cancellationToken)
    {
        var feedbacks = await _service.FeedbackService.GetFeedbacksBySubmissionIdAsync(submissionId, cancellationToken);

        return Ok(feedbacks);
    }

    [HttpPost]
    [Authorize(Roles = UserRoles.Teacher)]
    public async Task<ActionResult> CreateFeedback(
    [FromBody] FeedbackFormDto createDto,
    CancellationToken cancellationToken)
    {
        // Automatically extract the logged-in teacher's ID from claims
        var teacherId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                    ?? throw new UnauthorizedAccessException("Teacher ID could not be found from claims.");

        await _service.FeedbackService.CreateFeedbackAsync(createDto, teacherId, cancellationToken);

        return Ok(true);
    }
}
