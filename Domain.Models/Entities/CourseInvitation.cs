using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Models.Entities;

public class CourseInvitation
{
    public int Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public int CourseId { get; set; }
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public bool IsUsed { get; set; } = false;
}