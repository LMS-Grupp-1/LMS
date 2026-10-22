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
				
		[HttpGet("my")]
		[Authorize(Roles = UserRoles.Student)]
		public async Task<ActionResult<IReadOnlyList<StudentAssignmentDto>>> GetMyAssignments()
		{
			var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
			if (userId is null) return Unauthorized();

			var assignments = await _assignmentService.GetMyAssignmentsAsync(userId);
			return Ok(assignments);
		}
	}
}
