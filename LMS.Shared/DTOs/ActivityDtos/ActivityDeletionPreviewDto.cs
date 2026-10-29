using System;
using System.Collections.Generic;
using System.Text;

namespace LMS.Shared.DTOs.ActivityDtos;
    public record ActivityDeletionPreviewDto(
        int ActivityId,
        string ActivityName,
        IReadOnlyList<ActivityDeletionDocumentDto> Documents,
        IReadOnlyList<ActivityDeletionAssignmentDto> Assignments)
    {
        public bool RequiresAdditionalConfirmation =>
            Assignments.Any(assignment =>
                assignment.SubmissionCount > 0 ||
                assignment.FeedbackCount > 0);
    }
