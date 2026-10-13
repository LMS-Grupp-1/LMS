namespace LMS.Shared.DTOs.ActivityDtos;

public record ActivityDto(
    int Id,
  //  public int ModuleId { get; set; } // add Module?
    string ActivityType,
    string Name,
    string Description ,
    DateTime StartDate,
    DateTime EndDate
);