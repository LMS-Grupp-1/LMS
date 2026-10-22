using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace LMS.Presentation.Models
{
	// The form sent when a student hands in a file.
	public sealed class SubmissionUploadRequest
	{
		[Required(ErrorMessage = "Välj en fil att lämna in.")]
		public IFormFile? File { get; set; }
	}
}
