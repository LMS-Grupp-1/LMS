using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Models.Exceptions.CourseExceptions;
    public sealed class CourseNameAlreadyExistsException : ConflictException
    {
        public CourseNameAlreadyExistsException()
            : base("A course with this name already exists.")
        { }
    }
