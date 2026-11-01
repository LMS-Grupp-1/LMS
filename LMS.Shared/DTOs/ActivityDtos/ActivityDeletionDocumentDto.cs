using System;
using System.Collections.Generic;
using System.Text;

namespace LMS.Shared.DTOs.ActivityDtos;

public record ActivityDeletionDocumentDto(
    int Id,
    string Name,
    string FileName);

