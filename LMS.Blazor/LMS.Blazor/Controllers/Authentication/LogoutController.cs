using LMS.Blazor.Services.Authentication.Tokens;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LMS.Blazor.Controllers.Authentication;

[Authorize]
[Route("Account")]
public sealed class LogoutController(ITokenStorage tokenStorage) : Controller
{
    [HttpPost("Logout")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout([FromForm] string? returnUrl)
    {
        var sessionId = User.FindFirstValue(ClaimTypes.Sid);

        if (!string.IsNullOrWhiteSpace(sessionId))
        {
            await tokenStorage.RemoveTokensAsync(sessionId);
        }

        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

        var localReturnUrl = string.IsNullOrWhiteSpace(returnUrl)
            ? "/"
            : $"/{returnUrl.TrimStart('/')}";

        return Url.IsLocalUrl(localReturnUrl)
            ? LocalRedirect(localReturnUrl)
            : LocalRedirect("/");
    }
}
