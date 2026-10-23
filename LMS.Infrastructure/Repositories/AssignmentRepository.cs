using Domain.Contracts;
using Domain.Models.Entities;
using LMS.Infrastructure.Data;
using LMS.Shared.DTOs;
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

        public async Task<IReadOnlyList<DashboardAssignmentDto>> GetActiveAssignmentsAsync()
        {
            var currentDate = DateTime.Today;

            return await _context.Assignments
                .AsNoTracking()
                .Where(assignment => assignment.Deadline != null && assignment.Deadline >= currentDate)
                .OrderBy(assignment => assignment.Deadline)
                .Select(assignment => new DashboardAssignmentDto(
                    CourseName: assignment.Module.Course.Name,
                    AssignmentName: assignment.Name,
                    Deadline: assignment.Deadline
                ))
                .ToListAsync();
        }
    }
}
