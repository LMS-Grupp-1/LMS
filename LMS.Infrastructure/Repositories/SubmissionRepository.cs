using Domain.Contracts;
using Domain.Models.Entities;
using LMS.Infrastructure.Data;
using LMS.Shared.DTOs;
using Microsoft.EntityFrameworkCore;

namespace LMS.Infrastructure.Repositories
{
	public class SubmissionRepository(ApplicationDbContext context) : ISubmissionRepository
	{
		private readonly ApplicationDbContext _context = context;

		public async Task<Submission?> GetByIdAsync(int id)
		{
			return await _context.Submissions.FindAsync(id);
		}

		public async Task<Submission?> GetWithDocumentAsync(int id)
		{
			return await _context.Submissions
				.AsNoTracking()
				.Include(submission => submission.Document)
				.Include(submission => submission.Assignment)
					.ThenInclude(assignment => assignment.Module)
				.FirstOrDefaultAsync(submission => submission.Id == id);
		}

        public async Task<IReadOnlyList<DashboardSubmissionDto>?> GetLatestAsync(DateTime? afterDate)
        {
            var query = _context.Submissions
                .AsNoTracking()
                .Include(submission => submission.Assignment)
                .Include(submission => submission.Student)
                .AsQueryable();

            if (afterDate.HasValue)
            {
                query = query.Where(submission => submission.SubmittedAt > afterDate.Value);
            }

            return await query
				.OrderByDescending(submission => submission.SubmittedAt)
				.Select(submission => new DashboardSubmissionDto(
					submission.Assignment.Name,
					submission.Student.FirstName + " " + submission.Student.LastName,
					submission.SubmittedAt
				))
				.ToListAsync();
        }

        public async Task<IReadOnlyList<SubmissionStatisticsDto>?> GetSubmissionStatisticsByCourseIdAsync(int courseId)
        {
            return await _context.Assignments
                .AsNoTracking()
                .Where(assignment => assignment.Module.CourseId == courseId && assignment.RequiresSubmission)
                .Select(assignment => new SubmissionStatisticsDto
                (
                    assignment.Name,
                    assignment.Deadline,
                    assignment.Submissions.Count,
                    assignment.Submissions.Count(s => s.Feedback != null)
                ))
                .ToListAsync();
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
