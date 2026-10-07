using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace LMS.Shared.DTOs.CourseDtos
{
    public record CourseCreateDto
    {
        [Required(ErrorMessage = "Course name is required.")]
        [StringLength(50, ErrorMessage = "Course name cannot exceed 50 characters.")]
        public string Name { get; init; } = string.Empty;

        public string Description { get; init; } = string.Empty;

        [Required(ErrorMessage = "Start date is required.")]
        public DateTime StartDate { get; init; }

        [Required(ErrorMessage = "End date is required.")]
        public DateTime EndDate { get; init; }
    }
}
