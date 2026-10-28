using Domain.Contracts;
using Domain.Models.Entities;
using LMS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace LMS.Infrastructure.Repositories;

public class FeedbackRepository : BaseRepository<Feedback>, IFeedbackRepository
{
    public FeedbackRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Feedback>> GetFeedbacksBySubmissionIdAsync(int submissionId, CancellationToken cancellationToken = default)
    {
        return await Set
            .Where(f => f.SubmissionId == submissionId)
            .Include(f => f.Teacher)
            .ToListAsync(cancellationToken);
    }
}