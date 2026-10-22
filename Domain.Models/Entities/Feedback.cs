namespace Domain.Models.Entities
{
	public class Feedback
	{
		public int Id { get; set; }

		public int SubmissionId { get; set; }
		public Submission Submission { get; set; } = null!;

		public string TeacherId { get; set; } = string.Empty;
		public ApplicationUser Teacher { get; set; } = null!;

		public string Comment { get; set; } = string.Empty;
		public DateTimeOffset CreatedAt { get; set; }
	}
}
