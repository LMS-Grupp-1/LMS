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
	public class DashboardController: ControllerBase
	{
        private readonly IServiceManager _service;

        public DashboardController(IServiceManager service)
        {
            _service = service;
        }

        [HttpGet("student")]
		[Authorize(Roles = UserRoles.Student)]
		public async Task<ActionResult<StudentDashboardDto>> GetStudentDashboard(CancellationToken cancellationToken)
		{
			var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
			if (userId is null) return Unauthorized();

			var dashboard = await _service.DashboardService.GetStudentDashboardAsync(userId, cancellationToken);
			return Ok(dashboard);
        }

        [HttpGet("teacher/active-courses")]
        [Authorize(Roles = UserRoles.Teacher)]
        public async Task<ActionResult<IReadOnlyList<TeacherDashboardCourseDto>>> GetActiveCourses([FromQuery] int? count = null)
        {
            var courses = await _service.DashboardService.GetActiveCoursesAsync(count);
            return Ok(courses);
        }

        [HttpGet("teacher/upcoming-activities")]
        [Authorize(Roles = UserRoles.Teacher)]
        public async Task<ActionResult<IReadOnlyList<TeacherDashboardActivityDto>>> GetUpcomingActivities(
            [FromQuery] int? count = null)
        {
            var activities = await _service.DashboardService.GetUpcomingActivitiesAsync(count);
            return Ok(activities);
        }

        [HttpGet("teacher/active-assignments")]
        [Authorize(Roles = UserRoles.Teacher)]
        public async Task<ActionResult<IReadOnlyList<TeacherDashboardAssignmentDto>>> GetActiveAssignments([FromQuery] int? count = null)
        {
            var assignments = await _service.DashboardService.GetActiveAssignmentsAsync(count);
            return Ok(assignments);
        }

        [HttpGet("teacher/latest-submissions")]
        [Authorize(Roles = UserRoles.Teacher)]
        public async Task<ActionResult<IReadOnlyList<TeacherDashboardSubmissionDto>>> GetLatestSubmissions([FromQuery] DateTime? afterDate = null)
        {
            var submissions = await _service.DashboardService.GetLatestSubmissionsAsync(afterDate);
            return Ok(submissions);
        }
    }
}
