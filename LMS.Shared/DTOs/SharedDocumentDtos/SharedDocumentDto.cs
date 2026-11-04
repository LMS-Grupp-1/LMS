namespace LMS.Shared.DTOs.SharedDocumentDtos
{
    public record SharedDocumentDto(
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
        string Location
    );

    // 2. Used as a payload class when uploading/sending data from the Front End
    public class UploadSharedDocumentDto
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int CourseId { get; set; }
        public int? ModuleId { get; set; }
        public int? GroupId { get; set; }
    }
}