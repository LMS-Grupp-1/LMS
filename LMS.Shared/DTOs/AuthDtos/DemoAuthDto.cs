namespace LMS.Shared.DTOs.AuthDtos;

public sealed record DemoAuthDto(
    string Message,
    string? UserName,
    IReadOnlyCollection<string> Roles,
    DateTimeOffset RequestedAtUtc);
