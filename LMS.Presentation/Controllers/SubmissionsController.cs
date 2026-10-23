using LMS.Shared.Constants;
using LMS.Shared.DTOs.AssignmentDtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service.Contracts;
using System.Security.Claims;

namespace LMS.Presentation.Controllers
{
	[ApiController]
	[Route("api")]
	[Authorize]	
	[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
	public class SubmissionsController(IServiceManager serviceManager) : ControllerBase
	{
		private readonly ISubmissionService _submissionService = serviceManager.SubmissionService;
		
		
		[HttpGet("courses/{courseId:int}/submissions")]
		[Authorize(Roles = UserRoles.Teacher)]
		public async Task<ActionResult<IReadOnlyList<TeacherSubmissionRowDto>>> GetForCourse(
			int courseId,
			[FromQuery] int? assignmentId,
			[FromQuery] AssignmentStatus? status)
		{
			var rows = await _submissionService.GetForCourseAsync(courseId, assignmentId, status);
			return Ok(rows);
		}
				
		[HttpGet("submissions/{id:int}/download")]
		[Authorize(Roles = UserRoles.Teacher + "," + UserRoles.Student)]
		public async Task<IActionResult> Download(int id, CancellationToken cancellationToken)
		{
			var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
			if (userId is null) return Unauthorized();

			var download = await _submissionService.DownloadAsync(
				id, userId, User.IsInRole(UserRoles.Teacher), cancellationToken);
						
			Response.Headers["X-Content-Type-Options"] = "nosniff";
						
			return File(download.Content, download.ContentType, download.FileName);
		}
	}
}