using System;
using System.Collections.Generic;
using System.Text;

namespace LMS.Shared.DTOs.ActivityDtos;
    public class ActivityDeleteRequestDto
    {
        public string Version { get; set; } = string.Empty;
        public bool ConfirmDeletion { get; set; }
        public bool ConfirmStudentWorkDeletion { get; set; }
    }
