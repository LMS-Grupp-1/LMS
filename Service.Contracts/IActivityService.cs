using LMS.Shared.DTOs.ActivityDtos;

namespace Service.Contracts;

public interface IActivityService
{
    Task<IEnumerable<ActivityDto>> GetActivitesByModuleIdAsync(int moduleId);
    Task<IEnumerable<ActivityDto>> GetActivitesByModuleIdForUserAsync(int moduleId,
        string userId, CancellationToken cancellationToken = default);

    Task CreateActivityAsync(int moduleId,ActivityFormDto dto);

    Task UpdateActivityAsync(int moduleId, int activityId, ActivityFormDto dto);

    Task DeleteActivityAsync(
        int moduleId,
        int activityId,
        ActivityDeleteRequestDto request);

    Task<ActivityDeletionPreviewDto> GetDeletionPreviewAsync(
        int moduleId,
        int activityId);
}