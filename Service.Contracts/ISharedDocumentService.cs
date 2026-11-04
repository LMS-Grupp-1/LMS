using Domain.Models.Documents;
using LMS.Shared.DTOs.SharedDocumentDtos;
using Microsoft.AspNetCore.Http;

namespace Service.Contracts;

public interface ISharedDocumentService
{
    Task<IReadOnlyList<SharedDocumentDto>> GetByCourseIdAsync(int courseId, CancellationToken cancellationToken);
    Task<IReadOnlyList<SharedDocumentDto>> GetForUserAsync(string userId, CancellationToken cancellationToken);

    Task CreateForUserAsync(
        string userId,
        UploadSharedDocumentDto dto,
        Stream content,
        string fileName,
        CancellationToken cancellationToken = default);

    Task<DocumentDownload> DownloadAsync(
        int id,
        string userId,
        bool isStudent,
        CancellationToken cancellationToken);

    Task DeleteAsync(
        int id,
        string userId,
        bool isStudent,
        CancellationToken cancellationToken);
}
