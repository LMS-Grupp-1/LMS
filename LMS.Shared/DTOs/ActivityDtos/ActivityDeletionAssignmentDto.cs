using System;
using System.Collections.Generic;
using System.Text;

namespace LMS.Shared.DTOs.ActivityDtos;
public record ActivityDeletionAssignmentDto(
    int Id,
    string Name,
    int SubmissionCount,
    int FeedbackCount);