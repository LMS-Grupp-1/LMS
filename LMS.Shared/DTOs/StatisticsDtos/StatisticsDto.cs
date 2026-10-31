using System;
using System.Collections.Generic;
using System.Text;

namespace LMS.Shared.DTOs.StatisticsDtos;

public record ModuleStatisticsDto(
    int ActivityCount,
    int AssignmentCount,
    int DocumentCount
);
