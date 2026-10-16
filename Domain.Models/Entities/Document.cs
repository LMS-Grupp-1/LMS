using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Models.Entities
{
    public class Document
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty; //example: kursplan
        public string ContentType { get; set; } = string.Empty; //example:pdf
        public long SizeBytes { get; set; }
        public string BlobPath { get; set; } = string.Empty;
        public DateTimeOffset UploadedAt { get; set; }
        public string UploadedById { get; set; } = string.Empty;
        public ApplicationUser UploadedBy { get; set; } = null!;
        public int? CourseId { get; set; }
        public Course? Course { get; set; }
        public int? ModuleId { get; set; }
        public Module? Module { get; set; }
        public int? ActivityId { get; set; }
        public Activity? Activity { get; set; }
    }
}
