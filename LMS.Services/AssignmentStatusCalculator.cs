using LMS.Shared.DTOs.AssignmentDtos;

namespace LMS.Services
{
	// Pure status logic: no database, no clock. Everything comes in as parameters so it is easy to test.
	public static class AssignmentStatusCalculator
	{
		// Deadlines are entered and stored in Swedish local time.
		private static readonly TimeZoneInfo SwedishTime =
			TimeZoneInfo.FindSystemTimeZoneById("Europe/Stockholm");

		// Turns a stored deadline (Swedish local time, no offset) into an exact point in time.
		public static DateTimeOffset ToSwedishTime(DateTime localTime) =>
			new(localTime, SwedishTime.GetUtcOffset(localTime));

		// The opposite of ToSwedishTime: the current moment as Swedish local time,
		// for comparing with stored activity times, module dates and deadlines.
		public static DateTime ToSwedishLocalTime(DateTimeOffset moment) =>
			TimeZoneInfo.ConvertTime(moment, SwedishTime).DateTime;

		public static AssignmentStatus Calculate(
			bool requiresSubmission,
			DateTimeOffset? deadline,
			DateTimeOffset? submittedAt,
			DateTimeOffset now)
		{
			if (!requiresSubmission)
			{
				return AssignmentStatus.NoSubmissionRequired;
			}

			if (submittedAt is not null)
			{
				// Handing in exactly at the deadline counts as on time.
				return deadline is not null && submittedAt > deadline
					? AssignmentStatus.Late
					: AssignmentStatus.Submitted;
			}

			return deadline is not null && now > deadline
				? AssignmentStatus.Late
				: AssignmentStatus.NotSubmitted;
		}
	}
}
