using Domain.Models.Entities;
using LMS.Shared.DTOs.CourseDtos;
using LMS.Shared.DTOs.DashboardDtos;

namespace Domain.Contracts
{
	public interface ISubmissionRepository : IBaseRepository<Submission>
	{
		Task<Submission?> GetWithDocumentAsync(int id);
        Task<IReadOnlyList<TeacherDashboardSubmissionDto>> GetLatestAsync(DateTime? afterDate);
		Task<IReadOnlyList<SubmissionStatisticsDto>> GetSubmissionStatisticsByCourseIdAsync(int courseId);
    }
}
