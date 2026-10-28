using LMS.Shared.DTOs.FeedbackDtos;
using System;
using System.Collections.Generic;
using System.Text;

namespace Service.Contracts;

public interface IFeedbackService
{
    Task<IEnumerable<FeedbackDto>> GetFeedbacksBySubmissionIdAsync(int submissionId, CancellationToken cancellationToken = default);
    Task CreateFeedbackAsync(FeedbackFormDto dto, string teacherId, CancellationToken cancellationToken = default);
}
