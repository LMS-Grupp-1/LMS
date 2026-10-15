using System;
using System.Collections.Generic;
using System.Text;

namespace LMS.Shared.DTOs.DocumentDtos
{
    public record DocumentDto( //Information needed to show a document
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
        int? ActivityId,
        string Location);

    public record DocumentTarget( //Connections for the document
        int? CourseId = null,
        int? ModuleId = null,
        int? ActivityId = null);

    public record DocumentTargetDto( //Ensures a readble label
        DocumentTarget Target,
        string Label);
}
