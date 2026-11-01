namespace Domain.Models.Exceptions.AssignmentExceptions
{
	// 409: deleting would also remove the students' handed in files.
	public sealed class AssignmentHasSubmissionsException : ConflictException
	{
		public AssignmentHasSubmissionsException()
			: base("Uppgiften har inlämningar och kan inte tas bort.")
		{
		}
	}
}
