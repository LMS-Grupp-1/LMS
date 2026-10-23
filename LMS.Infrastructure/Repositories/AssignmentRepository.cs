using Domain.Contracts;
using Domain.Models.Entities;
using LMS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LMS.Infrastructure.Repositories
{
	public class AssignmentRepository(ApplicationDbContext context) : IAssignmentRepository
	{
		private readonly ApplicationDbContext _context = context;

		public async Task<IReadOnlyList<Assignment>> GetForStudentAsync(int courseId, string studentId)
		{
			return await _context.Assignments
				.AsNoTracking()
				.Include(assignment => assignment.Module)				
				.Include(assignment => assignment.Submissions
					.Where(submission => submission.StudentId == studentId))
					.ThenInclude(submission => submission.Feedback)
				.Where(assignment => assignment.Module.CourseId == courseId)
				.OrderBy(assignment => assignment.Deadline == null)
				.ThenBy(assignment => assignment.Deadline)
				.ThenBy(assignment => assignment.Name)
				.ToListAsync();
		}

		public async Task<Assignment?> GetByIdForStudentAsync(int id, string studentId)
		{
			return await _context.Assignments
				.AsNoTracking()
				.Include(assignment => assignment.Module)
				// Filtered include: other students' submissions are never loaded.
				.Include(assignment => assignment.Submissions
					.Where(submission => submission.StudentId == studentId))
					.ThenInclude(submission => submission.Feedback)
				.FirstOrDefaultAsync(assignment => assignment.Id == id);
		}

		public async Task<IReadOnlyList<Assignment>> GetWithSubmissionsForCourseAsync(int courseId)
		{
			return await _context.Assignments
				.AsNoTracking()
				.Include(assignment => assignment.Module)
				// All students' submissions, with the file name and whether feedback exists.
				.Include(assignment => assignment.Submissions)
					.ThenInclude(submission => submission.Document)
				.Include(assignment => assignment.Submissions)
					.ThenInclude(submission => submission.Feedback)
				.Where(assignment =>
					assignment.Module.CourseId == courseId &&
					assignment.RequiresSubmission)
				.OrderBy(assignment => assignment.Deadline == null)
				.ThenBy(assignment => assignment.Deadline)
				.ThenBy(assignment => assignment.Name)
				.ToListAsync();
		}
	}
}
