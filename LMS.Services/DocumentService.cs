using Domain.Contracts;
using Domain.Models.Entities;
using Domain.Models.Exceptions;
using LMS.Shared.Constants;
using LMS.Shared.DTOs.DocumentDtos;
using Microsoft.Extensions.Logging;
using Service.Contracts;

namespace LMS.Services;

public sealed class DocumentService(
    IUnitOfWork unitOfWork,
    IDocumentStorage storage,
    ILogger<DocumentService> logger) : IDocumentService
{
    public async Task<IReadOnlyList<DocumentDto>> ListAsync(
        int courseId,
        DocumentActor actor,
        CancellationToken cancellationToken)
    {
        await RequireCourseAccessAsync(
            courseId, actor, cancellationToken);

        return await unitOfWork.Documents.ListAsync(
            courseId, cancellationToken);
    }

    public async Task<IReadOnlyList<DocumentTargetDto>> TargetsAsync(
        int courseId,
        DocumentActor actor,
        CancellationToken cancellationToken)
    {
        RequireTeacher(actor);

        await RequireCourseAccessAsync(
            courseId, actor, cancellationToken);

        return await unitOfWork.Documents.TargetsAsync(
            courseId, cancellationToken);
    }

    public async Task<DocumentDto> UploadAsync(
        DocumentTarget target,
        string name,
        string? description,
        string fileName,
        Stream content,
        DocumentActor actor,
        CancellationToken cancellationToken)
    {
        RequireTeacher(actor);
        ValidateTarget(target);

        name = (name ?? string.Empty).Trim();
        description = (description ?? string.Empty).Trim();

        if (name.Length == 0 ||
            name.Length > DocumentConstraints.MaxNameLength)
        {
            throw new DocumentValidationException(
                $"Name must contain between 1 and " +
                $"{DocumentConstraints.MaxNameLength} characters.");
        }

        if (description.Length >
            DocumentConstraints.MaxDescriptionLength)
        {
            throw new DocumentValidationException(
                $"Description cannot exceed " +
                $"{DocumentConstraints.MaxDescriptionLength} characters.");
        }

        fileName = (fileName ?? string.Empty)
            .Replace('\\', '/');

        fileName = fileName[(fileName.LastIndexOf('/') + 1)..].Trim();

        if (string.IsNullOrWhiteSpace(fileName) ||
            fileName.Length > DocumentConstraints.MaxFileNameLength ||
            fileName.Any(char.IsControl))
        {
            throw new DocumentValidationException(
                "The file name is invalid or too long.");
        }

        string extension = Path.GetExtension(fileName);

        if (!DocumentConstraints.ContentTypes.TryGetValue(
                extension, out var contentType))
        {
            throw new DocumentValidationException(
                "This file type is not allowed.");
        }

        int? courseId = await unitOfWork.Documents.ResolveCourseAsync(
            target, cancellationToken);

        if (courseId is null)
        {
            throw new DocumentNotFoundException(
                "The selected course, module or activity does not exist.");
        }

        string? uploaderName =
            await unitOfWork.Documents.GetUploaderNameAsync(
                actor.UserId, cancellationToken);

        if (uploaderName is null)
        {
            throw new DocumentAccessException();
        }

        var targets = await unitOfWork.Documents.TargetsAsync(
            courseId.Value, cancellationToken);

        var selectedTarget = targets.FirstOrDefault(
            item => item.Target == target);

        if (selectedTarget is null)
        {
            throw new DocumentNotFoundException(
                "The selected document location does not exist.");
        }

        using var bufferedContent = await ReadFileAsync(
            content, cancellationToken);

        var document = new Document
        {
            Name = name,
            Description = description,
            FileName = fileName,
            ContentType = contentType,
            SizeBytes = bufferedContent.Length,
            UploadedAt = DateTimeOffset.UtcNow,
            UploadedById = actor.UserId,
            BlobPath = Guid.NewGuid().ToString("N"),
            CourseId = target.CourseId,
            ModuleId = target.ModuleId,
            ActivityId = target.ActivityId
        };

        await storage.SaveAsync(
            document.BlobPath,
            bufferedContent,
            document.ContentType,
            cancellationToken);

        try
        {
            await unitOfWork.Documents.AddAsync(
                document, cancellationToken);

            await unitOfWork.CompleteAsync();
        }
        catch
        {
            // Remove the uploaded file if saving its metadata fails.
            try
            {
                await storage.DeleteAsync(
                    document.BlobPath, CancellationToken.None);
            }
            catch (Exception cleanupException)
            {
                logger.LogError(
                    cleanupException,
                    "Could not remove blob {BlobPath} after a failed save.",
                    document.BlobPath);
            }

            throw;
        }

        return new DocumentDto(
            document.Id,
            document.Name,
            document.Description,
            document.FileName,
            document.ContentType,
            document.SizeBytes,
            document.UploadedAt,
            uploaderName,
            courseId.Value,
            document.ModuleId,
            document.ActivityId,
            selectedTarget.Label);
    }

    public async Task<DocumentDownload> DownloadAsync(
        int id,
        DocumentActor actor,
        CancellationToken cancellationToken)
    {
        RequireSignedIn(actor);

        var document = await unitOfWork.Documents.FindAsync(
            id, cancellationToken);

        if (document is null)
        {
            throw new DocumentNotFoundException(
                "The document does not exist.");
        }

        var target = new DocumentTarget(
            document.CourseId,
            document.ModuleId,
            document.ActivityId);

        // This service only handles teaching material with one target.
        if (CountTargets(target) != 1)
        {
            throw new DocumentAccessException();
        }

        int? courseId = await unitOfWork.Documents.ResolveCourseAsync(
            target, cancellationToken);

        if (courseId is null)
        {
            throw new DocumentAccessException();
        }

        await RequireCourseAccessAsync(
            courseId.Value, actor, cancellationToken);

        Stream stream = await storage.OpenReadAsync(
            document.BlobPath, cancellationToken);

        // The endpoint sending the response must dispose this stream.
        return new DocumentDownload(
            stream,
            document.ContentType,
            document.FileName);
    }

    private async Task RequireCourseAccessAsync(
        int courseId,
        DocumentActor actor,
        CancellationToken cancellationToken)
    {
        RequireSignedIn(actor);

        if (!actor.IsTeacher)
        {
            if (!actor.IsStudent)
            {
                throw new DocumentAccessException();
            }

            int? studentCourseId =
                await unitOfWork.Documents.GetStudentCourseAsync(
                    actor.UserId, cancellationToken);

            if (studentCourseId != courseId)
            {
                throw new DocumentAccessException();
            }
        }

        int? existingCourseId =
            await unitOfWork.Documents.ResolveCourseAsync(
                new DocumentTarget(CourseId: courseId),
                cancellationToken);

        if (existingCourseId is null)
        {
            throw new DocumentNotFoundException(
                "The course does not exist.");
        }
    }

    private static void RequireSignedIn(DocumentActor actor)
    {
        if (string.IsNullOrWhiteSpace(actor.UserId))
        {
            throw new DocumentAccessException();
        }
    }

    private static void RequireTeacher(DocumentActor actor)
    {
        RequireSignedIn(actor);

        if (!actor.IsTeacher)
        {
            throw new DocumentAccessException();
        }
    }

    private static int CountTargets(DocumentTarget target)
    {
        return (target.CourseId.HasValue ? 1 : 0)
             + (target.ModuleId.HasValue ? 1 : 0)
             + (target.ActivityId.HasValue ? 1 : 0);
    }

    private static void ValidateTarget(DocumentTarget target)
    {
        if (CountTargets(target) != 1 ||
            target.CourseId is <= 0 ||
            target.ModuleId is <= 0 ||
            target.ActivityId is <= 0)
        {
            throw new DocumentValidationException(
                "Select one course, module or activity.");
        }
    }

    private static async Task<MemoryStream> ReadFileAsync(
        Stream content,
        CancellationToken cancellationToken)
    {
        var result = new MemoryStream();

        try
        {
            byte[] buffer = new byte[81920];
            int bytesRead;

            while ((bytesRead = await content.ReadAsync(
                       buffer.AsMemory(), cancellationToken)) > 0)
            {
                if (result.Length + bytesRead >
                    DocumentConstraints.MaxFileSize)
                {
                    throw new DocumentValidationException(
                        $"The file cannot exceed {DocumentConstraints.MaxFileSize} bytes.");
                }

                await result.WriteAsync(
                    buffer.AsMemory(0, bytesRead),
                    cancellationToken);
            }

            if (result.Length == 0)
            {
                throw new DocumentValidationException(
                    "The file cannot be empty.");
            }

            result.Position = 0;
            return result;
        }
        catch
        {
            result.Dispose();
            throw;
        }
    }
}