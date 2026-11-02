namespace Domain.Models.Entities
{
	public class Submission
	{
		public int Id { get; set; }

		public int AssignmentId { get; set; }
		public Assignment Assignment { get; set; } = null!;

		public string StudentId { get; set; } = string.Empty;
		public ApplicationUser Student { get; set; } = null!;

		public int DocumentId { get; set; }
		public Document Document { get; set; } = null!;

		// Set by the server when the file is uploaded, never by the client.
		public DateTimeOffset SubmittedAt { get; set; }

        public ICollection<Feedback> Feedbacks { get; set; } = new List<Feedback>();
    }
}
