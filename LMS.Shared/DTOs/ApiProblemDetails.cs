using System;
using System.Collections.Generic;
using System.Text;

namespace LMS.Shared.DTOs;

public class ApiProblemDetails
{
    public string? Title { get; set; }
    public string? Detail { get; set; }
    public int? Status { get; set; }
    public Dictionary<string, string[]>? Errors { get; set; }
}