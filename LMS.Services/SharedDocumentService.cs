using Domain.Contracts;
using Domain.Models.Documents;
using Domain.Models.Entities;
using Domain.Models.Exceptions;
using LMS.Shared.Constants;
using LMS.Shared.DTOs.ActivityDtos;
using LMS.Shared.DTOs.DocumentDtos;
using LMS.Shared.DTOs.SharedDocumentDtos;
using Service.Contracts;
using System.Xml.Linq;

namespace LMS.Services;

public class SharedDocumentService : ISharedDocumentService
{
    private readonly IUnitOfWork _unitOfWork;
    private IDocumentStorage _storage;

    public SharedDocumentService(IUnitOfWork unitOfWork,
    IDocumentStorage storage)
    {
        _unitOfWork = unitOfWork;
        _storage = storage;
    }

    public async Task<IReadOnlyList<SharedDocumentDto>> GetByCourseIdAsync(int courseId, CancellationToken cancellationToken)
    {
        var result = await GetDtoListByCourseId(courseId, cancellationToken);

        return result;
    }

    public async Task<IReadOnlyList<SharedDocumentDto>> GetForUserAsync(string userId, CancellationToken cancellationToken)
    {
        int? courseId = await _unitOfWork.Users.GetUserCourseIdAsync(userId, cancellationToken);
        if (!courseId.HasValue)
        {
            throw new PropertyNotFoundException("The user has not been registered to any course yet.");
        }

        var result = await GetDtoListByCourseId(courseId.Value, cancellationToken);

        return result;
    }

    public async Task CreateForUserAsync(
        string userId,
        UploadSharedDocumentDto dto,
        Stream content,
        string fileName,
        CancellationToken cancellationToken = default)
    {
        int? userCourseId = await _unitOfWork.Users.GetUserCourseIdAsync(userId, cancellationToken);

        if (!userCourseId.HasValue)
        {
            throw new PropertyNotFoundException("The user has not been registered to any course yet.");
        }

        // Can not upload documents to other courses
        if (userCourseId.Value != dto.CourseId)
        {
            throw new ForbiddenException("Students are not allowed to upload documents to other courses.");
        }

        // Can not upload documents to other modules in other courses
        if (dto.ModuleId.HasValue)
        {
            var module = await _unitOfWork.Modules.GetByIdAsync(dto.ModuleId.Value);
            
            if (module is null)
            {
                throw new EntityNotFoundException("Module", dto.ModuleId.Value);
            }
            if (module.CourseId != dto.CourseId)
            {
                throw new ForbiddenException("Students are not allowed to upload documents to other courses.");
            }
        }

        if (dto.Name.Length == 0 ||
            dto.Name.Length > DocumentConstraints.MaxNameLength)
        {
            throw new BadRequestException(
                $"Name must contain between 1 and " +
                $"{DocumentConstraints.MaxNameLength} characters.");
        }

        if (dto.Description.Length >
            DocumentConstraints.MaxDescriptionLength)
        {
            throw new BadRequestException(
                $"Description cannot exceed " +
                $"{DocumentConstraints.MaxDescriptionLength} characters.");
        }

        // Process the file if provided
        // Add file storage logic (e.g., saving to wwwroot or cloud blob storage)
        var (cleanFileName, contentType) = DocumentFiles.ValidateFileName(fileName);
        fileName = cleanFileName;

        using var bufferedContent = await DocumentFiles.ReadAsync(
        content, cancellationToken);

        // Create the entity and persist to database
        var document = new SharedDocument
        {
            Name = dto.Name,
            Description = dto.Description,
            CourseId = dto.CourseId,
            ModuleId = dto.ModuleId,
            GroupId = dto.GroupId,
            FileName = fileName,
            ContentType = contentType,
            SizeBytes = bufferedContent.Length,
            BlobPath = Guid.NewGuid().ToString("N"),
            UploadedById = userId,
            UploadedAt = DateTimeOffset.UtcNow
        };

        await _storage.SaveAsync(
            document.BlobPath,
            bufferedContent,
            document.ContentType,
            cancellationToken);

        await _unitOfWork.SharedDocuments.AddAsync(document);
        await _unitOfWork.CompleteAsync();
    }

    public async Task<DocumentDownload> DownloadAsync(
        int id,
        string userId,
        bool isStudent,
        CancellationToken cancellationToken)
    {
        var document = await _unitOfWork.SharedDocuments.GetByIdAsync(id);

        if (document is null)
        {
            throw new EntityNotFoundException("Document", id);
        }

        if (isStudent)
        {
            int? userCourseId = await _unitOfWork.Users.GetUserCourseIdAsync(userId, cancellationToken);
            if (!userCourseId.HasValue)
            {
                throw new PropertyNotFoundException("The user has not been registered to any course yet.");
            }
            else if (userCourseId.Value != document.CourseId)
            {
                throw new ForbiddenException("Students are not allowed to download documents from other courses.");
            }
        }

        Stream stream = await _storage.OpenReadAsync(
            document.BlobPath, cancellationToken);

        // The endpoint sending the response must dispose this stream.
        return new DocumentDownload(
            stream,
            document.ContentType,
            document.FileName);
    }

    public async Task DeleteAsync(
        int id,
        string userId,
        bool isStudent,
        CancellationToken cancellationToken)
    {
        var document = await _unitOfWork.SharedDocuments.GetByIdAsync(id);
        if (document is null)
        {
            throw new EntityNotFoundException("Document", id);
        }

        if (isStudent && document.UploadedById != userId)
        {
            throw new ForbiddenException("You are not allowed to delete other users' document.");
        }
        string blobPath = document.BlobPath;

        _unitOfWork.SharedDocuments.Delete(document);
        await _unitOfWork.CompleteAsync();

        await _storage.DeleteAsync(blobPath, CancellationToken.None);
    }

    private async Task<IReadOnlyList<SharedDocumentDto>> GetDtoListByCourseId(int courseId, CancellationToken cancellationToken)
    {
        var course = await _unitOfWork.Courses.GetByIdAsync(courseId);
        if (course is null)
        {
            throw new EntityNotFoundException("Course", courseId);
        }

        var result = await _unitOfWork.SharedDocuments.GetByCourseIdAsync(courseId, cancellationToken);

        return result.Select(sd => new SharedDocumentDto(
                sd.Id,
                sd.Name,
                sd.Description,
                sd.FileName,
                sd.ContentType,
                sd.SizeBytes,
                sd.UploadedAt,
                sd.UploadedBy != null ? $"{sd.UploadedBy.FirstName} {sd.UploadedBy.LastName}" : "Unknown",
                courseId,
                sd.ModuleId,
                sd.GroupId,
                sd.ModuleId.HasValue
                    ? $"Modul: {sd.Module?.Name ?? sd.ModuleId.ToString()}{(sd.GroupId.HasValue ? $" / Grupp: {sd.GroupId}" : "")}"
                    : $"Kursen{(sd.GroupId.HasValue ? $" / Grupp {sd.GroupId}" : "")}"
        )).ToList();
    }
}
