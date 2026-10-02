namespace LMS.Shared.DTOs.AuthDtos;

public sealed record AuthenticatedUserDto(IReadOnlyCollection<UserClaimDto> Claims);
