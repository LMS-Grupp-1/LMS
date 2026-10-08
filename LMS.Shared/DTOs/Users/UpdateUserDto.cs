using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace LMS.Shared.DTOs.Users;

public class UpdateUserDto
{
    public string Id { get; set; } = string.Empty;

    [Required(ErrorMessage = "E-post är obligatorisk.")]
    [EmailAddress(ErrorMessage = "Ogiltigt e-postformat.")]
    public string Email { get; set; } = string.Empty;


    [Required(ErrorMessage = "Roll måste väljas.")]
    public string Role { get; set; } = null!;

    public string? Name { get; set; } = string.Empty;

    public string? Course { get; set; }
}
