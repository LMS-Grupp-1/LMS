namespace Domain.Models.Exceptions
{
	public sealed class AssignmentNotFoundException : NotFoundException
	{
		public AssignmentNotFoundException()
			: base("The assignment does not exist.", "Assignment not found")
		{
		}
	}
}
