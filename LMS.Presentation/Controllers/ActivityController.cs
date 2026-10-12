using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service.Contracts;

namespace LMS.Presentation;

[Authorize(Roles = "Teacher")]
[ApiController]
[Route("api/[controller]")]
public class ActivityController(IServiceManager serviceManager) : ControllerBase
{
   // private readonly ICourseService _courseService = serviceManager.CourseService;
}