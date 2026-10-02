using LMS.Shared.DTOs.AuthDtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Service.Contracts;
using Swashbuckle.AspNetCore.Annotations;

namespace LMS.Presentation.Controllers;

[Route("api/auth")]
[ApiController]
[Consumes("application/json")]
[Produces("application/json")]
public class AuthController(IServiceManager serviceManager) : ControllerBase
{
    private readonly IServiceManager _serviceManager = serviceManager;


    [HttpPost("login")]
    [AllowAnonymous]
    [SwaggerOperation(
        Summary = "Authenticate user",
        Description = "Validates user credentials and returns a JWT token for authorization.")]
    [SwaggerResponse(StatusCodes.Status200OK, "Authentication successful", typeof(TokenDto))]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, "Invalid username or password")]
    public async Task<ActionResult<TokenDto>> Authenticate(UserAuthDto userDto)
    {
        var token = await _serviceManager.AuthService.AuthenticateAsync(userDto);
        if (token is null)
            return Unauthorized();

        return Ok(token);
    }

    [HttpGet("me")]
    [Authorize]
    [SwaggerOperation(
        Summary = "Get authenticated user",
        Description = "Returns the claims of the currently authenticated user.")]
    [SwaggerResponse(StatusCodes.Status200OK,"Authenticated user retrieved successfully", typeof(AuthenticatedUserDto))]
    [SwaggerResponse(StatusCodes.Status401Unauthorized,"User is not authenticated")]
    public ActionResult<AuthenticatedUserDto> GetAuthenticatedUser()
    {
        var claims = User.Claims
            .Select(claim => new UserClaimDto(claim.Type, claim.Value))
            .ToArray();

        return Ok(new AuthenticatedUserDto(claims));
    }
}
