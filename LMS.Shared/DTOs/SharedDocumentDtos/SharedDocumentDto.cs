namespace LMS.Shared.DTOs.SharedDocumentDtos
{
    public record SharedDocumentDto( //Information needed to show a SharedDocument
        int Id,
        string Name,
        string Description,
        string FileName,
        string ContentType,
        long SizeBytes,
        DateTimeOffset UploadedAt,
        string UploadedByName,
        int CourseId,
        int? ModuleId,
        int? GroupId,
        string Location);
}
