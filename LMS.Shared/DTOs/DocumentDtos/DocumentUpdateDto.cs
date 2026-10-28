using System;
using System.Collections.Generic;
using System.Text;

namespace LMS.Shared.DTOs.DocumentDtos;
    public sealed record DocumentUpdateDto(
        string Name,
        string? Description,
        DocumentTarget Target);
