using Domain.Contracts;
using Domain.Models.Entities;
using LMS.Infrastructure.Data;
using LMS.Shared.DTOs.CourseDtos;
using LMS.Shared.DTOs.DashboardDtos;
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
					.ThenInclude(submission => submission.Feedbacks)
						.ThenInclude(feedback => feedback.Teacher)
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
					.ThenInclude(submission => submission.Feedbacks)
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
					.ThenInclude(submission => submission.Feedbacks)
				.Where(assignment =>
					assignment.Module.CourseId == courseId &&
					assignment.RequiresSubmission)
				.OrderBy(assignment => assignment.Deadline == null)
				.ThenBy(assignment => assignment.Deadline)
				.ThenBy(assignment => assignment.Name)
				.ToListAsync();
		}

        public async Task<IReadOnlyList<TeacherDashboardAssignmentDto>> GetActiveAssignmentsAsync(int? count = null)
        {
            var currentDate = DateTime.Today;

            var query = _context.Assignments
                .AsNoTracking()
                .Where(assignment => assignment.Deadline != null && assignment.Deadline >= currentDate)
                .OrderBy(assignment => assignment.Deadline)
                .Select(assignment => new TeacherDashboardAssignmentDto(
                    CourseName: assignment.Module.Course.Name,
                    AssignmentName: assignment.Name,
                    Deadline: assignment.Deadline
                ));

            if (count.HasValue && count.Value > 0)
            {
                query = query.Take(count.Value);
            }

            return await query.ToListAsync();
        }

        public async Task<AssignmentStatisticsDto?> GetAssignmentStatisticsForCourse(int courseId)
        {
            return await _context.Assignments
                .AsNoTracking()
                .Where(assignment => assignment.Module.CourseId == courseId)
                .GroupBy(assignment => 1)
                .Select(group => new AssignmentStatisticsDto(
                    group.Count(),
                    group.Count(a => a.RequiresSubmission),
                    group.Count(a => !a.RequiresSubmission)
                ))
                .FirstOrDefaultAsync();
        }
    }
}
