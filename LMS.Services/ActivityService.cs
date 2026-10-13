using Domain.Contracts;
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
//        throw new NotImplementedException();
        return result.Select(a => new ActivityDto(a.Id, 
            "Unimplemented",
            a.Name, 
            a.Description,
            a.StartDate,
            a.EndDate));
    }
}