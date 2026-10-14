using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace LMS.Shared.DTOs.ModulesDtos;

public class CreateModuleDto
{
    public int CourseId { get; set; }

    [Required(ErrorMessage = "Modulnamn är obligatoriskt.")]
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; } = string.Empty;

    [Required(ErrorMessage = "Startdatum är obligatoriskt.")]
    public DateTime StartDate { get; set; }

    [Required(ErrorMessage = "Slutdatum är obligatoriskt.")]
    public DateTime EndDate { get; set; }
}
