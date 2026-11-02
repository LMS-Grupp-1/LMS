namespace Domain.Models.Exceptions.SubmissionExceptions
{
	public sealed class SubmissionValidationException : BadRequestException
	{
		public SubmissionValidationException(string message) : base(message)
		{
		}
	}
}
