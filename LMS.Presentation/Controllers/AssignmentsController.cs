using Domain.Models.Exceptions.SubmissionExceptions;
using LMS.Presentation.Models;
using LMS.Shared.Constants;
using LMS.Shared.DTOs.AssignmentDtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service.Contracts;
using System.Security.Claims;

namespace LMS.Presentation.Controllers
{
	[ApiController]
	[Route("api/assignments")]
	[Authorize]
	public class AssignmentsController(IServiceManager serviceManager) : ControllerBase
	{
		private readonly IAssignmentService _assignmentService = serviceManager.AssignmentService;
		private readonly ISubmissionService _submissionService = serviceManager.SubmissionService;

		[HttpGet("my")]
		[Authorize(Roles = UserRoles.Student)]
		public async Task<ActionResult<IReadOnlyList<StudentAssignmentDto>>> GetMyAssignments()
		{
			var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
			if (userId is null) return Unauthorized();

			var assignments = await _assignmentService.GetMyAssignmentsAsync(userId);
			return Ok(assignments);
		}
				
		[HttpPost("{id:int}/submissions")]
		[Authorize(Roles = UserRoles.Student)]
		[Consumes("multipart/form-data")]
		[RequestSizeLimit(DocumentConstraints.MaxRequestSize)]
		[RequestFormLimits(MultipartBodyLengthLimit = DocumentConstraints.MaxRequestSize)]
		public async Task<ActionResult<StudentAssignmentDto>> Submit(
			int id,
			[FromForm] SubmissionUploadRequest request,
			CancellationToken cancellationToken)
		{
			var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
			if (userId is null) return Unauthorized();

			var file = request.File;

			// Quick checks before reading the file; the service checks the content again.
			if (file is null || file.Length == 0)
			{
				throw new SubmissionValidationException("The file cannot be empty.");
			}

			if (file.Length > DocumentConstraints.MaxFileSize)
			{
				throw new SubmissionValidationException(
					$"The file cannot exceed {DocumentConstraints.MaxFileSize} bytes.");
			}

			using var content = file.OpenReadStream();

			var assignment = await _submissionService.SubmitAsync(
				id, userId, file.FileName, content, cancellationToken);

			return Ok(assignment);
		}
	}
}
