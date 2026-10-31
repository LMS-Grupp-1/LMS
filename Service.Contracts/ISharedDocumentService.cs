using LMS.Shared.DTOs.SharedDocumentDtos;

namespace Service.Contracts;

public interface ISharedDocumentService
{
    Task<IReadOnlyList<SharedDocumentDto>> GetByCourseIdAsync(int courseId, CancellationToken cancellationToken);
    Task<IReadOnlyList<SharedDocumentDto>> GetForUserAsync(string userId, CancellationToken cancellationToken);
}
