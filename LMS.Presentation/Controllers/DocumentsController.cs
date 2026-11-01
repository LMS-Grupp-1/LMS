using System.Security.Claims;
using LMS.Presentation.Models;
using LMS.Shared.Constants;
using LMS.Shared.DTOs.DocumentDtos;
using Domain.Models.Exceptions;
using Domain.Models.Exceptions.DocumentExceptions;
using Domain.Models.Documents;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Service.Contracts;

namespace LMS.Presentation.Controllers;

[ApiController]
[Route("api/documents")]
[Authorize(Roles = UserRoles.Teacher + "," + UserRoles.Student)]
[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
public sealed class DocumentsController(IServiceManager serviceManager)
    : ControllerBase
{
    private readonly IDocumentService _documents =
        serviceManager.DocumentService;

    [HttpGet]
    public Task<IActionResult> GetDocuments(
        [FromQuery] int courseId,
        CancellationToken cancellationToken)
    {
        return ExecuteAsync(async actor =>
        {
            if (courseId <= 0)
                throw new DocumentValidationException(
                    "The course ID must be greater than zero.");

            var documents = await _documents.ListAsync(
                courseId, actor, cancellationToken);

            return Ok(documents);
        });
    }

    [HttpGet("targets")]
    [Authorize(Roles = UserRoles.Teacher)]
    public Task<IActionResult> GetTargets(
        [FromQuery] int courseId,
        CancellationToken cancellationToken)
    {
        return ExecuteAsync(async actor =>
        {
            if (courseId <= 0)
                throw new DocumentValidationException(
                    "The course ID must be greater than zero.");

            var targets = await _documents.TargetsAsync(
                courseId, actor, cancellationToken);

            return Ok(targets);
        });
    }

    [HttpPost]
    [Authorize(Roles = UserRoles.Teacher)]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(DocumentConstraints.MaxRequestSize)]
    [RequestFormLimits(
        MultipartBodyLengthLimit = DocumentConstraints.MaxRequestSize)]
    public Task<IActionResult> Upload(
        [FromForm] DocumentUploadRequest request,
        CancellationToken cancellationToken)
    {
        return ExecuteAsync(async actor =>
        {
            var file = request.File;

            if (file is null || file.Length == 0)
            {
                throw new DocumentValidationException(
                    "The file cannot be empty.");
            }

            if (file.Length > DocumentConstraints.MaxFileSize)
            {
                throw new DocumentValidationException(
                    $"The file cannot exceed {DocumentConstraints.MaxFileSize} bytes.");
            }

            var target = new DocumentTarget(
                request.CourseId,
                request.ModuleId,
                request.ActivityId);

            using var content = file.OpenReadStream();

            var document = await _documents.UploadAsync(
                target,
                request.Name,
                request.Description,
                file.FileName,
                content,
                actor,
                cancellationToken);

            return StatusCode(StatusCodes.Status201Created, document);
        });
    }

    [HttpGet("{id:int}/download")]
    public Task<IActionResult> Download(
        int id,
        CancellationToken cancellationToken)
    {
        return ExecuteAsync(async actor =>
        {
            if (id <= 0)
                throw new DocumentValidationException(
                    "The document ID must be greater than zero.");

            var document = await _documents.DownloadAsync(
                id, actor, cancellationToken);

            Response.Headers["X-Content-Type-Options"] = "nosniff";

            // ASP.NET Core disposes the stream after sending the response.
            return File(
                document.Content,
                document.ContentType,
                document.FileName);
        });
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = UserRoles.Teacher)]
    public Task<IActionResult> Update(
        int id,
        [FromBody] DocumentUpdateDto dto,
        CancellationToken cancellationToken)
    {
        return ExecuteAsync(async actor =>
        {
            await _documents.UpdateAsync(
                id, dto, actor, cancellationToken);

            return NoContent();
        });
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = UserRoles.Teacher)]
    public Task<IActionResult> Delete(
        int id,
        CancellationToken cancellationToken)
    {
        return ExecuteAsync(async actor =>
        {
            await _documents.DeleteAsync(
                id, actor, cancellationToken);

            return NoContent();
        });
    }

    private async Task<IActionResult> ExecuteAsync(
        Func<DocumentActor, Task<IActionResult>> action)
    {
        string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(userId))
        {
            throw new TokenValidationException(
                "The authenticated user identifier is missing.");
        }

        var actor = new DocumentActor(
            userId,
            User.IsInRole(UserRoles.Teacher),
            User.IsInRole(UserRoles.Student));

            return await action(actor);
        }
    }