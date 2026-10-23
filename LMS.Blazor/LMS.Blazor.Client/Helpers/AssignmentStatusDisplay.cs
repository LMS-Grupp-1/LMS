using LMS.Shared.DTOs.AssignmentDtos;

namespace LMS.Blazor.Client.Helpers
{
	public static class AssignmentStatusDisplay
	{
		// Text and colour for a student's assignment status.
		// Shared by "My assignments" and the student dashboard, so they always look the same.
		public static (string Text, string CssClass) For(AssignmentStatus status) => status switch
		{
			AssignmentStatus.Submitted => ("Inlämnad", "bg-success"),
			AssignmentStatus.NotSubmitted => ("Ej inlämnad", "bg-warning text-dark"),
			AssignmentStatus.Late => ("Försenad", "bg-danger"),
			AssignmentStatus.NoSubmissionRequired => ("Ingen inlämning", "bg-secondary"),
			_ => (status.ToString(), "bg-secondary")
		};
	}
}
