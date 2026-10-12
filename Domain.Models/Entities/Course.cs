namespace Domain.Models.Entities
{
	public class Course
	{
		public int Id { get; set; }
		public string Name { get; set; } = string.Empty;
		public string Description { get; set; } = string.Empty;
		public DateTime StartDate { get; set; }
		public DateTime EndDate { get; set; }

		// Navigation properties
		public ICollection<ApplicationUser> Students { get; set; } = new List<ApplicationUser>();

		//public ICollection<Module> Modules { get; set; } = new List<Module>();
		//public ICollection<Document> Documents { get; set; } = new List<Document>();
	}
}
