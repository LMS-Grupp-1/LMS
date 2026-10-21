using Domain.Contracts;
using Domain.Models.Entities;
using Domain.Models.Exceptions;
using LMS.Shared.DTOs.ActivityDtos;
using Service.Contracts;
using System.Xml.Linq;

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

    public async Task CreateActivityAsync(int moduleId, CreateActivityDto dto)
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