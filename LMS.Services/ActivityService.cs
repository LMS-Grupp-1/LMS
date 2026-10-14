using Domain.Contracts;
using Domain.Models.Entities;
using LMS.Shared.DTOs.ActivityDtos;
using Service.Contracts;

namespace LMS.Services;

public class ActivityService : IActivityService
{
    private readonly IUnitOfWork _unitOfWork;

    public ActivityService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }
    
    public async Task<IEnumerable<ActivityDto>> GetActivitesByModuleIdAsync(int moduleId)
    {
        var result = await _unitOfWork.Activities.GetActivityByModuleIdAsync(moduleId);
        return result.Select(a => new ActivityDto(a.Id, 
            "Unimplemented",
            a.Name, 
            a.Description,
            a.StartDate,
            a.EndDate));
    }

    public async Task CreateActivityAsync(int moduleId,CreateActivityDto dto)
    {
        Activity activity = new Activity
        {
            ModuleId = moduleId,
            Name = dto.Name,
            Description = dto.Description,
            StartDate = dto.StartDate,
            EndDate = dto.EndDate,
            ActivityTypeId = dto.ActivityTypeId
        };
        await _unitOfWork.Activities.AddAsync(activity);
        await _unitOfWork.CompleteAsync();
    }
}