namespace LMS.Shared.DTOs.AuthDtos;

public sealed record TokenDto(
    string AccessToken,
    string RefreshToken,
    DateTimeOffset AccessTokenExpiresAtUtc,
    DateTimeOffset RefreshTokenExpiresAtUtc);
