using Domain.Models.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Contracts;

public interface IFeedbackRepository : IBaseRepository<Feedback>
{
    Task<IEnumerable<Feedback>> GetFeedbacksBySubmissionIdAsync(int submissionId, CancellationToken cancellationToken = default);
}