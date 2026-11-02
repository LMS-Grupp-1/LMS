namespace Domain.Models.Exceptions.AssignmentExceptions
{
	public sealed class AssignmentNotFoundException : NotFoundException
	{
		public AssignmentNotFoundException()
			: base("The assignment does not exist.", "Assignment not found")
		{
		}
	}
}
