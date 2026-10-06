using System;
using System;
using System.Collections.Generic;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace LMS.Shared.DTOs.Users;

public record CreateUserDto
{
    [Required(ErrorMessage = "E-post är obligatorisk.")]
    [EmailAddress(ErrorMessage = "Ogiltigt e-postformat.")]
    public string Email { get; set; } = null!;

    [Required(ErrorMessage = "Lösenord är obligatoriskt.")]
    public string Password { get; set; } = null!;

    [Required(ErrorMessage = "Roll måste väljas.")]
    public string Role { get; set; } = null!;

    public string? Name { get; set; }
    public string? Course { get; set; }
}
