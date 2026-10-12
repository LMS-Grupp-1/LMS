using System;
using System.Collections.Generic;
using System.Text;

namespace LMS.Shared.DTOs.ModulesDtos;

public class CreateModuleDto
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}
