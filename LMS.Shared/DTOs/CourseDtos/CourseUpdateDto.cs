using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace LMS.Shared.DTOs.CourseDtos;
public record CourseUpdateDto(
    [Required(ErrorMessage = "Course name is required.")]
    [StringLength(50, ErrorMessage = "Course name cannot exceed 50 characters.")]
    string Name,

    string Description,

    [Required(ErrorMessage = "Start date is required.")]
    DateTime StartDate,

    [Required(ErrorMessage = "End date is required.")]
    DateTime EndDate
);
