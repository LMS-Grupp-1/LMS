using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace LMS.Shared.DTOs.CourseDtos;
    public record CourseCreateDto(
        [Required(ErrorMessage = "Kursnamn måste anges.")]
        [StringLength(50, ErrorMessage = "Kursnamnet får inte överskrida 50 tecken.")]
        string Name,
        string Description,

        [Required(ErrorMessage = "Startdatum måste anges.")]
        DateTime StartDate,

        [Required(ErrorMessage = "Slutdatum måste anges.")]
        DateTime EndDate
        );
