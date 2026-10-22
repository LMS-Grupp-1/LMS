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
	}
}
