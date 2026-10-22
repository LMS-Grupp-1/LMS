using LMS.Shared.DTOs.AssignmentDtos;

namespace LMS.Blazor.Client.Services
{
	public interface ISubmissionService
	{		
		Task<IReadOnlyList<TeacherSubmissionRowDto>?> GetForCourseAsync(
			int courseId,
			CancellationToken cancellationToken = default);
	}
}
