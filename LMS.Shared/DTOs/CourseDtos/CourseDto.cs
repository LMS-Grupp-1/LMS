using System;
using System.Collections.Generic;
using System.Text;

namespace LMS.Shared.DTOs.CourseDtos;

public record CourseDto(
    int Id,
    string Name,
    string Description,
    DateTime StartDate,
    DateTime EndDate
);
