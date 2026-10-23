namespace Domain.Models.Exceptions
{
	public sealed class SubmissionNotFoundException : NotFoundException
	{
		public SubmissionNotFoundException()
			: base("The submission does not exist.", "Submission not found")
		{
		}
	}
}
