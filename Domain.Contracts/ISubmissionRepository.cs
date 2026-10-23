using Domain.Models.Entities;
using LMS.Shared.DTOs;

namespace Domain.Contracts
{
	public interface ISubmissionRepository : IBaseRepository<Submission>
	{
		Task<Submission?> GetWithDocumentAsync(int id);
        Task<IReadOnlyList<DashboardSubmissionDto>> GetLatestAsync(DateTime? afterDate);
    }
}
