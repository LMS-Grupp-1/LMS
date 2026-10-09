using Microsoft.AspNetCore.Identity;

namespace Domain.Models.Entities;

public class ApplicationUser : IdentityUser
{    
	public string FirstName { get; set; } = string.Empty;
	public string LastName { get; set; } = string.Empty;

	// Null for teachers, set for students
	public int? CourseId { get; set; }
	public Course? Course { get; set; }

    public string? RefreshToken { get; set; }
    public DateTime RefreshTokenExpireTime { get; set; }

}
