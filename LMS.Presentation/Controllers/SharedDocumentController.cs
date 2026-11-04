using Domain.Models.Exceptions;
using LMS.Shared.Constants;
using LMS.Shared.DTOs.ActivityDtos;
using LMS.Shared.DTOs.ModulesDtos;
using LMS.Shared.DTOs.SharedDocumentDtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
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

    // Binds the "File" part from multipart content
    [HttpPost("for-student")]
    public async Task<IActionResult> CreateSharedDocumentForUser(
        IFormFile file,
        [FromForm] UploadSharedDocumentDto dto,
        CancellationToken cancellationToken = default)
    {
        if (file is null || file.Length == 0)
        {
            throw new BadRequestException("The file cannot be empty.");
        }
        if (file.Length > DocumentConstraints.MaxFileSize)
        {
            throw new BadRequestException(
                $"The file cannot exceed {DocumentConstraints.MaxFileSize} bytes.");
        }
        // Open the stream from file:
        using var fileStream = file.OpenReadStream();

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        await _service.SharedDocumentService.CreateForUserAsync(userId, dto, fileStream, file.FileName, cancellationToken);
        return Ok(true);
    }

    [HttpGet("{id:int}/download")]
    public async Task<IActionResult> Download(
        int id,
        CancellationToken cancellationToken)
    {
        if (id <= 0)
            throw new BadRequestException(
                "The document ID must be greater than zero.");

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }
        bool isStudent = User.IsInRole(UserRoles.Student);

        var document = await _service.SharedDocumentService.DownloadAsync(
                id, userId, isStudent, cancellationToken);

        Response.Headers["X-Content-Type-Options"] = "nosniff";

        // ASP.NET Core disposes the stream after sending the response.
        return File(
            document.Content,
            document.ContentType,
            document.FileName);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }
        bool isStudent = User.IsInRole(UserRoles.Student);

        await _service.SharedDocumentService.DeleteAsync(id, userId, isStudent, cancellationToken);
        return Ok(true);
    }
}
