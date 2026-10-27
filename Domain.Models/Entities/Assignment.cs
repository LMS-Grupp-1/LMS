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

		// Kept so existing queries can filter on module.
		// Always set from the activity's ModuleId in the service, never from the client.
		public int ModuleId { get; set; }
		public Module Module { get; set; } = null!;

		// Every assignment belongs to an activity in the same module.
		public int ActivityId { get; set; }
		public Activity Activity { get; set; } = null!;

		public ICollection<Submission> Submissions { get; set; } = new List<Submission>();
	}
}
