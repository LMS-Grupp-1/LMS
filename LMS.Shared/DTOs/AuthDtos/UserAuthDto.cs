using System.ComponentModel.DataAnnotations;

namespace LMS.Shared.DTOs.AuthDtos;

public record UserAuthDto
{
    [Required]
    public string Email { get; init; } = null!;

    [Required]
    public string Password { get; init; } = null!;
}
