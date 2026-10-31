using Domain.Contracts;
using Domain.Models.Entities;
using Domain.Models.Exceptions;
using LMS.Shared.DTOs.ActivityDtos;
using LMS.Shared.DTOs.ModulesDtos;
using Service.Contracts;
using System.Xml.Linq;
using Microsoft.Extensions.Logging;

namespace LMS.Services;

public class ActivityService : IActivityService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDocumentStorage _storage;
    private readonly ILogger<ActivityService> _logger;

    public ActivityService(
        IUnitOfWork unitOfWork,
        IDocumentStorage storage,
        ILogger<ActivityService> logger)
    {
        _unitOfWork = unitOfWork;
        _storage = storage;
        _logger = logger;
    }

    public async Task<IEnumerable<ActivityDto>> GetActivitesByModuleIdAsync(int moduleId)
    {
        var module = await _unitOfWork.Modules.GetByIdAsync(moduleId);

        if (module is null)
        {
            throw new ModuleNotFoundException(moduleId);
        }

        var result = await _unitOfWork.Activities.GetActivityByModuleIdAsync(moduleId);

        return result.Select(a => new ActivityDto(
            a.Id,
            a.ActivityType.Id,
			a.ActivityType.Name,
			a.Name,
            a.Description,
            a.StartDate,
            a.EndDate));
    }

    public async Task<IEnumerable<ActivityDto>> GetActivitesByModuleIdForUserAsync(int moduleId,
        string userId, CancellationToken cancellationToken = default)
    {
        int? courseId = await _unitOfWork.Users.GetUserCourseIdAsync(userId, cancellationToken);
        if (!courseId.HasValue)
        {
            throw new PropertyNotFoundException("The user has not been registered to any course yet.");
        }

        var course = await _unitOfWork.Courses.GetByIdAsync(courseId.Value, false);
        if (course == null)
            throw new EntityNotFoundException("Course", courseId.Value);

        var module = await _unitOfWork.Modules.GetByIdAsync(moduleId);

        if (module is null)
        {
            throw new ModuleNotFoundException(moduleId);
        }

        if (module.CourseId != course.Id)
        {
            throw new ForbiddenException("You don't have permission to access modules and activites in this course.");
        }

        var result = await _unitOfWork.Activities.GetActivityByModuleIdAsync(moduleId);

        return result.Select(a => new ActivityDto(
            a.Id,
            a.ActivityType.Id,
            a.ActivityType.Name,
            a.Name,
            a.Description,
            a.StartDate,
            a.EndDate));
    }

    public async Task CreateActivityAsync(int moduleId, ActivityFormDto dto)
    {
        await ValidateActivityAsync(
            moduleId,
            dto.Name,
            dto.StartDate,
            dto.EndDate);

        var activity = new Activity
        {
            ModuleId = moduleId,
            Name = dto.Name.Trim(),
            Description = dto.Description?.Trim() ?? string.Empty,
            StartDate = dto.StartDate,
            EndDate = dto.EndDate,
            ActivityTypeId = dto.ActivityTypeId
        };
        await _unitOfWork.Activities.AddAsync(activity);
        await _unitOfWork.CompleteAsync();
    }

    public async Task UpdateActivityAsync(int moduleId, int activityId, ActivityFormDto dto)
    {
        var activity = await _unitOfWork.Activities.GetByIdAsync(activityId);
        if (activity == null || activity.ModuleId != moduleId)
        {
            throw new EntityNotFoundException("Activity", activityId);
        }

        await ValidateActivityAsync(
           moduleId,
           dto.Name,
           dto.StartDate,
           dto.EndDate,
           activityId);

        // Update entity properties
        activity.ActivityTypeId = dto.ActivityTypeId;
        activity.Name = dto.Name;
        activity.Description = string.IsNullOrEmpty(dto.Description) ? "Ingen beskrivning" : dto.Description;
        activity.StartDate = dto.StartDate;
        activity.EndDate = dto.EndDate;

        _unitOfWork.Activities.Update(activity);
        await _unitOfWork.CompleteAsync();
    }

    public async Task<ActivityDeletionPreviewDto> GetDeletionPreviewAsync(
        int moduleId,
        int activityId)
    {
        var activity = await _unitOfWork.Activities.GetForDeletionAsync(
            moduleId,
            activityId);

        if (activity is null)
        {
            throw new EntityNotFoundException("Activity", activityId);
        }

        return ActivityDeletionPreviewFactory.Create(activity);
    }

    public async Task DeleteActivityAsync(
    int moduleId,
    int activityId,
    ActivityDeleteRequestDto request)
    {
        if (!request.ConfirmDeletion)
        {
            throw new BadRequestException(
                "Activity deletion must be explicitly confirmed.");
        }

        var activity = await _unitOfWork.Activities.GetForDeletionAsync(
            moduleId,
            activityId);

        if (activity is null)
        {
            throw new EntityNotFoundException("Activity", activityId);
        }

        var preview = ActivityDeletionPreviewFactory.Create(activity);

        if (string.IsNullOrWhiteSpace(request.Version) ||
            request.Version != preview.Version)
        {
            throw new ActivityDeletionChangedException();
        }

        if (preview.RequiresAdditionalConfirmation &&
            !request.ConfirmStudentWorkDeletion)
        {
            throw new BadRequestException(
                "Deleting student submissions and feedback must be explicitly confirmed.");
        }

        var assignments = activity.Assignments.ToList();

        var submissions = assignments
            .SelectMany(assignment => assignment.Submissions)
            .ToList();

        var feedbacks = submissions
            .SelectMany(submission => submission.Feedbacks)
            .ToList();

        var documents = activity.Documents
            .Concat(submissions.Select(submission => submission.Document))
            .DistinctBy(document => document.Id)
            .ToList();

        var blobPaths = documents
            .Select(document => document.BlobPath)
            .Distinct()
            .ToList();

        foreach (var feedback in feedbacks)
        {
            _unitOfWork.Feedbacks.Delete(feedback);
        }

        foreach (var submission in submissions)
        {
            _unitOfWork.Submissions.Delete(submission);
        }

        foreach (var assignment in assignments)
        {
            _unitOfWork.Assignments.Delete(assignment);
        }

        foreach (var document in documents)
        {
            _unitOfWork.Documents.Delete(document);
        }

        _unitOfWork.Activities.Delete(activity);

        await _unitOfWork.CompleteAsync();

        foreach (string blobPath in blobPaths)
        {
            try
            {
                await _storage.DeleteAsync(blobPath, CancellationToken.None);
            }
            catch (Exception exception)
            {
                _logger.LogError(
                    exception,
                    "Activity {ActivityId} was deleted, but blob {BlobPath} " +
                    "could not be removed. Storage cleanup is required.",
                    activityId,
                    blobPath);
            }
        }
    }

    private async Task ValidateActivityAsync(
        int moduleId,
        string? name,
        DateTime startDate,
        DateTime endDate,
        int? excludedActivityId = null)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ActivityNameRequiredException();
        }

        if (startDate == default ||
            endDate == default ||
            startDate >= endDate)
        {
            throw new InvalidActivityTimeRangeException();
        }

        var module = await _unitOfWork.Modules.GetByIdAsync(moduleId);

        if (module is null)
        {
            throw new ModuleNotFoundException(moduleId);

        }

        if (startDate.Date < module.StartDate.Date ||
            endDate.Date > module.EndDate.Date)
        {
            throw new ActivityOutsideModulePeriodException();
        }

        bool overlaps = await _unitOfWork.Activities.HasOverlapAsync(
            moduleId,
            startDate,
            endDate,
            excludedActivityId);

        if (overlaps)
        {
            throw new ActivityOverlapException();
        }
    }
}