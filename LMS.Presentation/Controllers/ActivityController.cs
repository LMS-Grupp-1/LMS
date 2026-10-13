using LMS.Shared.DTOs.ActivityDtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service.Contracts;
using Microsoft.EntityFrameworkCore;

namespace LMS.Presentation;

[Authorize(Roles = "Teacher")]
[ApiController]
[Route("api/teacher/courses/{courseId:int}/Modules/{moduleId:int}")]
public class ActivityController : ControllerBase
{
   // private readonly ICourseService _courseService = serviceManager.CourseService;
   private readonly IServiceManager _service;

   public ActivityController(IServiceManager service)
   {
      _service = service;
   }
   
   [HttpGet]
   public async Task<ActionResult<IEnumerable<ActivityDto>>> GetActivites(int moduleId)
   {
      var activites = await _service.ActivityService.GetActivitesByModuleIdAsync(moduleId);
      return Ok(null);
   }
}