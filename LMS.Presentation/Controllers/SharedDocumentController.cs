using LMS.Shared.Constants;
using LMS.Shared.DTOs.ActivityDtos;
using LMS.Shared.DTOs.SharedDocumentDtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service.Contracts;
using System.Security.Claims;
using System.Text;

namespace LMS.Presentation.Controllers;

[ApiController]
[Authorize]
[Route("api/shared-documents")]
public class SharedDocumentController : ControllerBase
{
    private readonly IServiceManager _service;

    public SharedDocumentController(IServiceManager service)
    {
        _service = service;
    }

    [HttpGet("in-course/{courseId:int}")]
    [Authorize(Roles = UserRoles.Teacher)]
    public async Task<ActionResult<IReadOnlyList<SharedDocumentDto>>> GetByCourseIdAsync(int courseId, CancellationToken cancellationToken)
    {
        var sharedDocuments = await _service.SharedDocumentService.GetByCourseIdAsync(courseId, cancellationToken);
        return Ok(sharedDocuments);
    }

    [HttpGet("for-student")]
    public async Task<ActionResult<IReadOnlyList<SharedDocumentDto>>> GetSharedDocumentsForUser(CancellationToken cancellationToken = default)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }
        var sharedDocuments = await _service.SharedDocumentService.GetForUserAsync(userId, cancellationToken);
        return Ok(sharedDocuments);
    }
}
