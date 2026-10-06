using System;
using System.Collections.Generic;
using System.Text;

namespace LMS.Shared.DTOs.Users;

public class UserDto
{
    public string Id { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string? Name { get; set; }
    public string? Course { get; set; }
}
