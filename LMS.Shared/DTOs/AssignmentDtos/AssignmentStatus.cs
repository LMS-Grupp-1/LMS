using System.Text.Json.Serialization;

namespace LMS.Shared.DTOs.AssignmentDtos
{
	// Serialized as text ("Late") instead of a number, so the JSON is readable in Swagger and logs.
	[JsonConverter(typeof(JsonStringEnumConverter<AssignmentStatus>))]
	public enum AssignmentStatus
	{
		// Requires submission, nothing handed in and the deadline has not passed (or there is no deadline).
		NotSubmitted,

		// Handed in on time, or the assignment has no deadline.
		Submitted,

		// Nothing handed in and the deadline has passed, or handed in after the deadline.
		Late,

		// Nothing to hand in, for example "read chapter 3".
		NoSubmissionRequired
	}
}
