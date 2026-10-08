using LMS.Shared.Constants;
using System.ComponentModel.DataAnnotations;


namespace LMS.Shared.DTOs.CourseDtos;

public record CourseUpdateDto(
	[Required(ErrorMessage = "Kursnamn måste anges.")]
	[StringLength(CourseConstraints.NameMaxLength, ErrorMessage = "Kursnamnet får inte överskrida {1} tecken.")]
	string Name,

	[StringLength(CourseConstraints.DescriptionMaxLength, ErrorMessage = "Kursbeskrivningen får inte överskrida {1} tecken.")]
	string Description,

	[Required(ErrorMessage = "Startdatum måste anges.")]
	DateTime StartDate,

	[Required(ErrorMessage = "Slutdatum måste anges.")]
	DateTime EndDate
);
