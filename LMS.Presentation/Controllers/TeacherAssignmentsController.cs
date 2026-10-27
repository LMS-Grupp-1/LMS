using LMS.Shared.Constants;
using LMS.Shared.DTOs.AssignmentDtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service.Contracts;

namespace LMS.Presentation.Controllers
{	
	[ApiController]
	[Route("api/teacher")]
	[Authorize(Roles = UserRoles.Teacher)]
	public class TeacherAssignmentsController(IServiceManager serviceManager) : ControllerBase
	{
		private readonly IAssignmentService _assignmentService = serviceManager.AssignmentService;

		[HttpGet("modules/{moduleId:int}/assignments")]
		public async Task<ActionResult<IReadOnlyList<TeacherAssignmentDto>>> GetForModule(int moduleId)
		{
			var assignments = await _assignmentService.GetForModuleAsync(moduleId);
			return Ok(assignments);
		}
				
		[HttpPost("assignments")]
		public async Task<IActionResult> Create([FromBody] AssignmentFormDto dto)
		{
			await _assignmentService.CreateAsync(dto);			
			return Ok(true);
		}

		[HttpPut("assignments/{id:int}")]
		public async Task<IActionResult> Update(int id, [FromBody] AssignmentFormDto dto)
		{
			await _assignmentService.UpdateAsync(id, dto);
			return Ok(true);
		}

		[HttpDelete("assignments/{id:int}")]
		public async Task<IActionResult> Delete(int id)
		{
			await _assignmentService.DeleteAsync(id);
			return Ok(true);
		}
	}
}
