using System;
using System.Collections.Generic;
using System.Text;

namespace LMS.Shared.Constants
{
    public static class DocumentConstraints
    {
        public const long MaxFileSize = 10 * 1024 * 1024;
        public const long MaxRequestSize = MaxFileSize + 1024 * 1024;
        public const int MaxFileNameLength = 255;
        public const int MaxContentTypeLength = 150;
        public const int MaxBlopPathLength = 450;

        public static readonly IReadOnlyDictionary<string, string> ContentTypes =
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                [".pdf"] = "application/pdf",
                [".txt"] = "text/plain",
                [".png"] = "image/png",
                [".jpg"] = "image/jpg",
                [".jpeg"] = "image/jpeg",
                [".docx"] = "application/vnd.openxmlformats-officedocument.wordpressingml.document",
                [".xlsx"] = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                [".pptx"] = "application/vnd.openxmlformats-officedocument.presentationml.presentation"
            };
    }
}
