using System.ComponentModel.DataAnnotations;
using LMS.Shared.Constants;

namespace LMS.Shared.DTOs.AssignmentDtos
{
	// Used both when creating and editing an assignment.
	public class AssignmentFormDto
	{		
		[Range(1, int.MaxValue, ErrorMessage = "Välj en aktivitet.")]
		public int ActivityId { get; set; }

		[Required(ErrorMessage = "Namn krävs.")]
		[StringLength(AssignmentConstraints.NameMaxLength, ErrorMessage = "Namnet får inte överstiga {1} tecken.")]
		public string Name { get; set; } = string.Empty;

		[StringLength(AssignmentConstraints.DescriptionMaxLength, ErrorMessage = "Beskrivningen får inte överstiga {1} tecken.")]
		public string? Description { get; set; }
				
		public DateTime? Deadline { get; set; }

		public bool RequiresSubmission { get; set; } = true;
	}
}
