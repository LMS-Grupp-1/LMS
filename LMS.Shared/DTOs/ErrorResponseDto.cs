using System;

namespace LMS.Shared.DTOs;

// New ApiProblemDetails Dto matches the structure of the ProblemDetails class used in ASP.NET Core for standardized error responses.
// It includes properties for Title, Detail, Status, and a dictionary for Errors, allowing for detailed error information to be conveyed in API responses.
public class ErrorResponseDto
{
	public string Message { get; set; } = string.Empty;
	public Dictionary<string, string[]>? Errors { get; set; }
}