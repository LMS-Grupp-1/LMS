
using LMS.Shared.Constants;
using System.ComponentModel.DataAnnotations;

namespace LMS.Shared.DTOs.CourseDtos
{
	public record CourseUpdateDto
	{
		[Required(ErrorMessage = "Course name is required.")]
		[StringLength(CourseConstraints.NameMaxLength, ErrorMessage = "Course name cannot exceed {1} characters.")]
		public string Name { get; init; } = string.Empty;
		
		[MaxLength(CourseConstraints.DescriptionMaxLength, ErrorMessage = "Course description cannot exceed {1} characters.")]
		public string Description { get; init; } = string.Empty;

		[Required(ErrorMessage = "Start date is required.")]
		public DateTime StartDate { get; init; }

		[Required(ErrorMessage = "End date is required.")]
		public DateTime EndDate { get; init; }
	}
}
