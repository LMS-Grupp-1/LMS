using System.Security.Claims;
using LMS.Presentation.Models;
using LMS.Shared.Constants;
using LMS.Shared.DTOs.DocumentDtos;
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
                return BadRequest(new { message = "Välj en giltig kurs." });

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
                return BadRequest(new { message = "Välj en giltig kurs." });

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
                return BadRequest(new { message = "Välj en fil som inte är tom." });

            if (file.Length > DocumentConstraints.MaxFileSize)
                return BadRequest(new
                {
                    message = $"Filen får vara högst " +
                    $"{DocumentConstraints.MaxFileSize / (1024 * 1024)} MB."
                });

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
                return BadRequest(new { message = "Dokumentets ID är ogiltigt." });

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

    private async Task<IActionResult> ExecuteAsync(
        Func<DocumentActor, Task<IActionResult>> action)
    {
        string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized(new
            {
                message = "Du behöver logga in för att fortsätta."
            });
        }


        var actor = new DocumentActor(
            userId,
            User.IsInRole(UserRoles.Teacher),
            User.IsInRole(UserRoles.Student));

        try
        {
            return await action(actor);
        }
        catch (DocumentValidationException ex)
        {
            string message = ex.ErrorCode switch
            {
                "InvalidName" =>
                    $"Namnet måste innehålla mellan 1 och " +
                    $"{DocumentConstraints.MaxNameLength} tecken.",

                "DescriptionTooLong" =>
                    $"Beskrivningen får innehålla högst " +
                    $"{DocumentConstraints.MaxDescriptionLength} tecken.",

                "InvalidFileName" =>
                    $"Filnamnet är ogiltigt eller innehåller fler än " +
                    $"{DocumentConstraints.MaxFileNameLength} tecken.",

                "FileTypeNotAllowed" =>
                    "Filtypen är inte tillåten.",

                "InvalidTarget" =>
                    "Välj en kurs, en modul eller en aktivitet.",

                "FileTooLarge" =>
                    $"Filen får vara högst " +
                    $"{DocumentConstraints.MaxFileSize / (1024 * 1024)} MB.",

                "EmptyFile" =>
                    "Filen är tom. Välj en fil med innehåll.",

                _ =>
                    "Dokumentet kunde inte sparas. Kontrollera uppgifterna."
            };

            return BadRequest(new { message });
        }
        catch (DocumentAccessException)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new
            {
                message = "Du har inte behörighet att komma åt detta material."
            });
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new
            {
                message = "Dokumentet eller den valda kursen, modulen eller aktiviteten finns inte."
            });
        }
        catch (FileNotFoundException)
        {
            return NotFound(new
            {
                message = "Filen kunde inte hittas i lagringen."
            });
        }
    }
}