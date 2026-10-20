using LMS.Shared.DTOs.ActivityDtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Domain.Models.Exceptions;
using LMS.Shared.Constants;
using Service.Contracts;

namespace LMS.Presentation;

[ApiController]
[Authorize]
[Route("api/teacher/courses/Modules/{moduleId:int}/Activities")]
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
      return Ok(activites);
   }

    [HttpPost]
    [Authorize(Roles = UserRoles.Teacher)]
    public async Task<IActionResult> CreateActivity(int moduleId, [FromBody] CreateActivityDto dto)
    {
        try
        {
            await _service.ActivityService.CreateActivityAsync(moduleId, dto);
            return NoContent();
        }
        catch (ActivityValidationException ex)
        {
            var (statusCode, message) = ex.ErrorCode switch
            {
                ActivityValidationError.InvalidName =>
                    (StatusCodes.Status400BadRequest,
                    "Ange ett namn på aktiviteten."),

                ActivityValidationError.InvalidTimeRange =>
                    (StatusCodes.Status400BadRequest,
                    "Ange start- och sluttid. Starttiden måste vara före sluttiden."),

                ActivityValidationError.ModuleNotFound =>
                    (StatusCodes.Status404NotFound,
                    "Den valda modulen finns inte."),

                ActivityValidationError.OutsideModulePeriod =>
                    (StatusCodes.Status400BadRequest,
                    "Aktiviteten måste ligga inom modulens start- och slutdatum."),

                ActivityValidationError.Overlap =>
                    (StatusCodes.Status409Conflict,
                    "Aktiviteten överlappar en annan aktivitet i samma modul."),
                _ =>
                (StatusCodes.Status400BadRequest,
                "Aktiviteten kunde inte sparas. Kontrollera uppgifterna.")
            };

            var problem = new ProblemDetails
            {
                Status = statusCode,
                Title = "Aktiviteten kunde inte sparas.",
                Detail = message
            };

            problem.Extensions["code"] = ex.ErrorCode.ToString();

            return StatusCode(statusCode, problem);
        }
    }
}