using System;
using System.Collections.Generic;
using System.Text;

namespace LMS.Shared.DTOs.DocumentDtos;
    public record DocumentTarget( //Connections for the document
        int? CourseId = null,
        int? ModuleId = null,
        int? ActivityId = null);
