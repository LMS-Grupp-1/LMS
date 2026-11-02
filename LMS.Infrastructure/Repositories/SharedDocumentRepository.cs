using Domain.Contracts;
using Domain.Models.Entities;
using LMS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LMS.Infrastructure.Repositories;

public class SharedDocumentRepository(ApplicationDbContext context)
    : BaseRepository<SharedDocument>(context), ISharedDocumentRepository
{
    public async Task<IReadOnlyList<SharedDocument>> GetByCourseIdAsync(
            int courseId,
            CancellationToken cancellationToken)
    {
        return await Set
            .AsNoTracking()
            .Where(sd => sd.CourseId == courseId)
            .Include(sd => sd.UploadedBy)
            .Include(sd => sd.Module)
            .OrderByDescending(sd => sd.UploadedAt)
            .ToListAsync(cancellationToken);
    }
}
