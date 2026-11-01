namespace Domain.Models.Exceptions.SubmissionExceptions
{
	public sealed class SubmissionNotFoundException : NotFoundException
	{
		public SubmissionNotFoundException()
			: base("The submission does not exist.", "Submission not found")
		{
		}
	}
}
