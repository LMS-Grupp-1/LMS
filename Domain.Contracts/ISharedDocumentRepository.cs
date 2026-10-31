using Domain.Models.Entities;

namespace Domain.Contracts
{
    public interface ISharedDocumentRepository : IBaseRepository<SharedDocument>
    {
        Task<IReadOnlyList<SharedDocument>> GetByCourseIdAsync(int courseId, CancellationToken cancellationToken);
    }
}
