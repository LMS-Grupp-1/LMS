using Domain.Models.Documents;
using LMS.Shared.DTOs.AssignmentDtos;

namespace Service.Contracts
{
	public interface ISubmissionService
	{		
		Task<StudentAssignmentDto> SubmitAsync(
			int assignmentId,
			string userId,
			string fileName,
			Stream content,
			CancellationToken cancellationToken);

		Task<IReadOnlyList<TeacherSubmissionRowDto>> GetForCourseAsync(
			int courseId,
			int? assignmentId,
			AssignmentStatus? status);
				
		Task<DocumentDownload> DownloadAsync(
			int submissionId,
			string userId,
			bool isTeacher,
			CancellationToken cancellationToken);
	}
}
