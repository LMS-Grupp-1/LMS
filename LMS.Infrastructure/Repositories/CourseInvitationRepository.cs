
using Domain.Contracts;
using Domain.Models.Entities;
using LMS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;

namespace LMS.Infrastructure.Repositories;

public class CourseInvitationRepository : BaseRepository<CourseInvitation>, ICourseInvitationRepository
{
    public CourseInvitationRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<CourseInvitation?> GetByEmailAndTokenAsync(string email, string token, CancellationToken cancellationToken = default)
    {
        return await Set
            .FirstOrDefaultAsync(i => i.Email == email && i.Token == token, cancellationToken);
    }
}
