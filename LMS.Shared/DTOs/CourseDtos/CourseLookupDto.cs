namespace LMS.Shared.DTOs.CourseDtos;

// For lightweight course lookups, e.g., for dropdown lists where only the ID and Name are needed.
public record CourseLookupDto(int Id, string Name);
