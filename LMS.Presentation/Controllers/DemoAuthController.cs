using LMS.Shared.DTOs.AuthDtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Security.Claims;

namespace LMS.Presentation.Controllers;

[ApiController]
[Authorize]
[Route("api/demo-auth")]
public sealed class DemoAuthController : ControllerBase
{
    [HttpGet]
    [SwaggerOperation(
        Summary = "Verify authentication",
        Description = "Verifies that the remote API accepts the access token and returns information about the authenticated user.")]
    [SwaggerResponse(StatusCodes.Status200OK, "Access token accepted", typeof(DemoAuthDto))]
    [SwaggerResponse(StatusCodes.Status401Unauthorized,"Access token is missing, invalid, or expired")]
    public ActionResult<DemoAuthDto> Get()
    {
        var roles = User.FindAll(ClaimTypes.Role)
            .Select(claim => claim.Value)
            .ToArray();

        return Ok(new DemoAuthDto(
            "The remote API accepted the access token.",
            User.Identity?.Name,
            roles,
            DateTimeOffset.UtcNow));
    }
}
