namespace LMS.Shared.DTOs.ActivityDtos;

public record ActivityDto(
    int Id,
    int ActivityTypeId,
    string ActivityType,
    string Name,
    string Description ,
    DateTime StartDate,
    DateTime EndDate
);