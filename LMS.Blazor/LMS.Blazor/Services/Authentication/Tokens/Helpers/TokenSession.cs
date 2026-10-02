using LMS.Shared.DTOs.AuthDtos;

namespace LMS.Blazor.Services.Authentication.Tokens.Helpers;

public sealed record TokenSession(string UserId, TokenDto Tokens);
