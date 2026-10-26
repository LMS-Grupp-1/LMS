using LMS.Shared.Constants;
using LMS.Shared.DTOs.CourseDtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service.Contracts;
using System.Security.Claims;

namespace LMS.Presentation.Controllers;

[ApiController]
[Route("api/courses")]
[Authorize]
public class CourseController(IServiceManager serviceManager) : ControllerBase
{
	private readonly ICourseService _courseService = serviceManager.CourseService;

    [HttpGet]
	[Authorize(Roles = UserRoles.Teacher)]
    public async Task<ActionResult<IEnumerable<CourseDto>>> GetCourses()
    {
        var courses = await _courseService.GetAllCoursesAsync();
        return Ok(courses);
    }

    [HttpGet("with-counts")]
    [Authorize(Roles = UserRoles.Teacher)]
    public async Task<ActionResult<IEnumerable<CourseDto>>> GetCoursesWithCounts()
    {
        var courses = await _courseService.GetCoursesWithCountsAsync();
        return Ok(courses);
    }

    [HttpGet("lookups")]
    [Authorize(Roles = UserRoles.Teacher)]
    public async Task<ActionResult<IEnumerable<CourseLookupDto>>> GetCourseLookups()
    {
        var courses = await _courseService.GetCourseLookupsAsync();
        return Ok(courses);
    }

    [HttpGet("{id:int}")]
	[Authorize(Roles = UserRoles.Teacher)]
    public async Task<ActionResult<CourseDto>> GetCourse(int id)
    {
        var course = await _courseService.GetCourseByIdAsync(id);

        return Ok(course);
    }

	[HttpGet("my")]
	[Authorize(Roles = UserRoles.Student)]
	public async Task<ActionResult<StudentCourseDto>> GetMyCourse()
	{		
		var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
		if (userId is null) return Unauthorized();

		var course = await _courseService.GetMyCourseAsync(userId);
		return Ok(course);
	}

    [HttpPost]
	[Authorize(Roles = UserRoles.Teacher)]
    public async Task<ActionResult<CourseDto>> CreateCourse([FromBody] CourseCreateDto courseCreateDto)
    {
        var createdCourse = await _courseService.CreateCourseAsync(courseCreateDto); 
        return CreatedAtAction(nameof(GetCourse), new { id = createdCourse.Id }, createdCourse);
    }

    [HttpPut("{id:int}")]
	[Authorize(Roles = UserRoles.Teacher)]
    public async Task<IActionResult> UpdateCourse(int id, [FromBody] CourseUpdateDto courseUpdateDto)
    {
        await _courseService.UpdateCourseAsync(id, courseUpdateDto);
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Teacher")] 
    public async Task<IActionResult> DeleteCourse(int id)
    {
        await _courseService.DeleteCourseAsync(id); 
        return NoContent();
    }
}
