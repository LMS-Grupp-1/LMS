using Domain.Contracts;
using Domain.Models.Entities;
using LMS.Infrastructure.Data;

namespace LMS.Infrastructure.Repositories
{
	public class SubmissionRepository(ApplicationDbContext context) : ISubmissionRepository
	{
		private readonly ApplicationDbContext _context = context;

		public async Task<Submission?> GetByIdAsync(int id)
		{
			return await _context.Submissions.FindAsync(id);
		}

		public async Task AddAsync(Submission submission)
		{
			await _context.Submissions.AddAsync(submission);
		}

		public void Update(Submission submission)
		{
			_context.Submissions.Update(submission);
		}

		public void Delete(Submission submission)
		{
			_context.Submissions.Remove(submission);
		}
	}
}
