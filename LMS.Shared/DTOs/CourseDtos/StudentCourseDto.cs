namespace LMS.Shared.DTOs.CourseDtos;

public record StudentCourseDto(
	int Id,
	string Name,
	string Description,
	DateTime StartDate,
	DateTime EndDate,
	IReadOnlyList<ParticipantDto> Participants);