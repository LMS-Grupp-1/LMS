using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Models.Exceptions.CourseExceptions;
    public sealed class CourseValidationException : BadRequestException
    {
    public CourseValidationException(string message) 
        : base(message) { }
    }
