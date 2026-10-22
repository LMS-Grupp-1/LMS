namespace LMS.Shared.DTOs.ActivityDtos;

public record CreateActivityDto(
    int ActivityTypeId,
    string Name,
    string Description , 
    DateTime StartDate,
    DateTime EndDate
);