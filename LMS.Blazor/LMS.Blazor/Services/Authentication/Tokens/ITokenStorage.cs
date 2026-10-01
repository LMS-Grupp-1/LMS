
using LMS.Blazor.Services.Authentication.Tokens.Helpers;
using LMS.Shared.DTOs.AuthDtos;

namespace LMS.Blazor.Services.Authentication.Tokens;

public interface ITokenStorage
{
    Task StoreTokensAsync(string sessionId, string userId, TokenDto tokens);
    Task<TokenSession?> GetSessionAsync(string sessionId);
    Task RemoveTokensAsync(string sessionId);
}
