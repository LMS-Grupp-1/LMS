namespace Domain.Models.Exceptions
{
	public sealed class SubmissionValidationException : BadRequestException
	{
		public SubmissionValidationException(string message) : base(message)
		{
		}
	}
}
