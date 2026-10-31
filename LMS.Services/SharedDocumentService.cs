using Domain.Contracts;
using Domain.Models.Entities;
using Domain.Models.Exceptions;
using LMS.Shared.DTOs.ActivityDtos;
using LMS.Shared.DTOs.SharedDocumentDtos;
using Service.Contracts;
using System.Reflection;

namespace LMS.Services;

public class SharedDocumentService : ISharedDocumentService
{
    private readonly IUnitOfWork _unitOfWork;

    public SharedDocumentService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
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
                sd.UploadedBy.FirstName + " " + sd.UploadedBy.LastName,
                courseId,
                sd.ModuleId,
                sd.GroupId,
                sd.CourseId != null
                    ? "Kursen"
                    : sd.Module != null
                        ? "Modul: " + sd.Module.Name
                        : sd.GroupId != null
                            ? "Grupp: Grupp" + sd.GroupId
                            : "")).ToList();
    }
}
