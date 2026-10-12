using LMS.Shared.Constants;
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

	[StringLength(UserConstraints.FirstNameMaxLength, ErrorMessage = "Förnamnet får inte överskrida {1} tecken.")]
	public string? FirstName { get; set; } = null!;
	[StringLength(UserConstraints.LastNameMaxLength, ErrorMessage = "Efternamnet får inte överskrida {1} tecken.")]
	public string? LastName { get; set; } = null!;
	public string? Course { get; set; }
}
