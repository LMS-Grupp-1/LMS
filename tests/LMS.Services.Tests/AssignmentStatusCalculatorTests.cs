using LMS.Shared.DTOs.AssignmentDtos;

namespace LMS.Services.Tests
{
	/// <summary>
	/// Unit tests for <see cref="AssignmentStatusCalculator"/>.
	/// </summary>
	/// <remarks>
	/// How these tests work:
	/// - The calculator is a pure function: it only uses its parameters and does not
	///   read the database or the clock. That is why no mocks are needed here.
	/// - "now" is passed in as a parameter, so each test can choose exactly where in time
	///   it is, for example one minute after the deadline.
	/// - All times are in UTC (TimeSpan.Zero) so they are easy to compare.
	///   The conversion from Swedish local time is tested separately at the bottom.
	/// </remarks>
	public class AssignmentStatusCalculatorTests
	{
		// The deadline used by most tests:
		// Friday 13 November 2026 17:00 Swedish time.
		// In November Sweden has winter time (UTC+1), so 17:00 Swedish time = 16:00 UTC.
		// The other times in the tests are written relative to this one,
		// for example Deadline.AddMinutes(1) = one minute too late.
		private static readonly DateTimeOffset Deadline = new(2026, 11, 13, 16, 0, 0, TimeSpan.Zero);

		/// <summary>
		/// Scenario: an assignment where nothing is handed in, for example "read chapter 3".
		/// Expected: NoSubmissionRequired, even after the deadline, since there is nothing to be late with.
		/// </summary>
		[Fact]
		public void Calculate_NoSubmissionRequired_ReturnsNoSubmissionRequired()
		{
			// Arrange: no submission is required, and "now" is one day after the deadline.
			var now = Deadline.AddDays(1);

			// Act
			var status = AssignmentStatusCalculator.Calculate(
				requiresSubmission: false, Deadline, submittedAt: null, now);

			// Assert: the deadline does not matter when nothing is to be handed in.
			Assert.Equal(AssignmentStatus.NoSubmissionRequired, status);
		}

		/// <summary>
		/// Scenario: the student has not handed in yet, but there is still time.
		/// Expected: NotSubmitted.
		/// </summary>
		[Fact]
		public void Calculate_NotSubmittedBeforeDeadline_ReturnsNotSubmitted()
		{
			// Arrange: "now" is one hour before the deadline, and nothing has been handed in.
			var now = Deadline.AddHours(-1);

			// Act
			var status = AssignmentStatusCalculator.Calculate(
				requiresSubmission: true, Deadline, submittedAt: null, now);

			// Assert
			Assert.Equal(AssignmentStatus.NotSubmitted, status);
		}

		/// <summary>
		/// Scenario: the deadline has passed and the student has not handed in.
		/// Expected: Late.
		/// </summary>
		[Fact]
		public void Calculate_NotSubmittedAfterDeadline_ReturnsLate()
		{
			// Arrange: "now" is one minute after the deadline, and nothing has been handed in.
			var now = Deadline.AddMinutes(1);

			// Act
			var status = AssignmentStatusCalculator.Calculate(
				requiresSubmission: true, Deadline, submittedAt: null, now);

			// Assert: one minute is enough to be late.
			Assert.Equal(AssignmentStatus.Late, status);
		}

		/// <summary>
		/// Scenario: an assignment that must be handed in, but has no deadline.
		/// Expected: NotSubmitted. Without a deadline an assignment can never be late.
		/// </summary>
		[Fact]
		public void Calculate_NoDeadlineAndNotSubmitted_ReturnsNotSubmitted()
		{
			// Arrange: no deadline (null), and "now" is a whole year later.
			var now = Deadline.AddYears(1);

			// Act
			var status = AssignmentStatusCalculator.Calculate(
				requiresSubmission: true, deadline: null, submittedAt: null, now);

			// Assert: still not late, since there is no deadline to pass.
			Assert.Equal(AssignmentStatus.NotSubmitted, status);
		}

		/// <summary>
		/// Scenario: the student handed in two hours before the deadline.
		/// Expected: Submitted.
		/// </summary>
		[Fact]
		public void Calculate_SubmittedBeforeDeadline_ReturnsSubmitted()
		{
			// Arrange: handed in two hours early. "now" is after the deadline,
			// which shows that the status depends on when the student handed in, not on today's date.
			var submittedAt = Deadline.AddHours(-2);
			var now = Deadline.AddDays(1);

			// Act
			var status = AssignmentStatusCalculator.Calculate(
				requiresSubmission: true, Deadline, submittedAt, now);

			// Assert
			Assert.Equal(AssignmentStatus.Submitted, status);
		}

		/// <summary>
		/// Scenario: the student handed in at exactly the deadline, to the second.
		/// Expected: Submitted. "Hand in by 17:00" includes 17:00.
		/// This is the boundary case: if the calculator used >= instead of > this test would fail.
		/// </summary>
		[Fact]
		public void Calculate_SubmittedExactlyAtDeadline_ReturnsSubmitted()
		{
			// Arrange: submittedAt is exactly the same moment as the deadline.
			var submittedAt = Deadline;
			var now = Deadline.AddDays(1);

			// Act
			var status = AssignmentStatusCalculator.Calculate(
				requiresSubmission: true, Deadline, submittedAt, now);

			// Assert
			Assert.Equal(AssignmentStatus.Submitted, status);
		}

		/// <summary>
		/// Scenario: the student handed in, but one second too late.
		/// Expected: Late. A late submission is still late, even though it exists.
		/// </summary>
		[Fact]
		public void Calculate_SubmittedAfterDeadline_ReturnsLate()
		{
			// Arrange: handed in one second after the deadline.
			var submittedAt = Deadline.AddSeconds(1);
			var now = Deadline.AddDays(1);

			// Act
			var status = AssignmentStatusCalculator.Calculate(
				requiresSubmission: true, Deadline, submittedAt, now);

			// Assert
			Assert.Equal(AssignmentStatus.Late, status);
		}

		/// <summary>
		/// Scenario: a teacher enters the deadline 17:00 in Swedish time.
		/// Expected: the deadline becomes 16:00 UTC in winter (UTC+1) and 15:00 UTC in summer (UTC+2).
		/// If the conversion ignored summer time, statuses would be one hour off half the year.
		/// </summary>
		/// <remarks>
		/// [Theory] runs the same test once for every [InlineData] row.
		/// The values in each row are passed to the method parameters in the same order.
		/// </remarks>
		[Theory]
		[InlineData(2026, 11, 13, 16)] // Winter time: 17:00 Swedish = 16:00 UTC
		[InlineData(2026, 6, 12, 15)]  // Summer time: 17:00 Swedish = 15:00 UTC
		public void ToSwedishTime_ConvertsLocalDeadlineToCorrectUtc(int year, int month, int day, int expectedUtcHour)
		{
			// Arrange: 17:00 on the given day, as it is stored in the database (no time zone).
			var localDeadline = new DateTime(year, month, day, 17, 0, 0);

			// Act
			var result = AssignmentStatusCalculator.ToSwedishTime(localDeadline);

			// Assert: compare the hour in UTC.
			Assert.Equal(expectedUtcHour, result.UtcDateTime.Hour);
		}
	}
}