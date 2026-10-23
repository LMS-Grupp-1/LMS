using Domain.Models.Entities;

namespace Domain.Contracts
{
	public interface ISubmissionRepository : IBaseRepository<Submission>
	{
		Task<Submission?> GetWithDocumentAsync(int id);
	}
}
