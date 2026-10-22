namespace Domain.Models.Entities
{
	public class Assignment
	{
		public int Id { get; set; }
		public string Name { get; set; } = string.Empty;
		public string Description { get; set; } = string.Empty;

		// Null means no deadline.
		public DateTime? Deadline { get; set; }

		// True when the students hand in a file.
		public bool RequiresSubmission { get; set; }

		public int ModuleId { get; set; }
		public Module Module { get; set; } = null!;

		public ICollection<Submission> Submissions { get; set; } = new List<Submission>();
	}
}
