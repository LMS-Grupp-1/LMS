using System.Security.Claims;
using LMS.Shared.Constants;
using LMS.Shared.DTOs.DashboardDtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service.Contracts;

namespace LMS.Presentation.Controllers
{
	[ApiController]
	[Route("api/dashboard")]
	[Authorize]
	public class DashboardController(IServiceManager serviceManager) : ControllerBase
	{
		private readonly IDashboardService _dashboardService = serviceManager.DashboardService;
				
		[HttpGet("student")]
		[Authorize(Roles = UserRoles.Student)]
		public async Task<ActionResult<StudentDashboardDto>> GetStudentDashboard(CancellationToken cancellationToken)
		{
			var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
			if (userId is null) return Unauthorized();

			var dashboard = await _dashboardService.GetStudentDashboardAsync(userId, cancellationToken);
			return Ok(dashboard);
		}
	}
}
