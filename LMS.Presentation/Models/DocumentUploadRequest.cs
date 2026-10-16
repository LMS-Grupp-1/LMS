using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace LMS.Presentation.Models
{
    public sealed class DocumentUploadRequest
    {
        [Required(ErrorMessage = "Ange ett namn på dokumentet.")]
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        [Required(ErrorMessage = "Välj en fil att ladda upp.")]
        public IFormFile? File { get; set; }
        public int? CourseId { get; set; }
        public int? ModuleId { get; set; }
        public int? ActivityId { get; set; }
    }
}
