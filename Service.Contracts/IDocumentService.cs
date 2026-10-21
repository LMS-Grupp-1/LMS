using LMS.Shared.DTOs.DocumentDtos;
using Domain.Models.Documents;

namespace Service.Contracts;

public interface IDocumentService
{
    Task<IReadOnlyList<DocumentDto>> ListAsync(
        int courseId,
        DocumentActor actor,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<DocumentTargetDto>> TargetsAsync(
        int courseId,
        DocumentActor actor,
        CancellationToken cancellationToken);

    Task<DocumentDto> UploadAsync(
        DocumentTarget target,
        string name,
        string? description,
        string fileName,
        Stream content,
        DocumentActor actor,
        CancellationToken cancellationToken);

    Task<DocumentDownload> DownloadAsync(
        int id,
        DocumentActor actor,
        CancellationToken cancellationToken);
}