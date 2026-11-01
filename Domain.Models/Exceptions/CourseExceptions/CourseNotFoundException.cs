using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Models.Exceptions.CourseExceptions;
    public sealed class CourseNotFoundException : NotFoundException
    {
        public CourseNotFoundException(int id)
            : base($"Course with ID {id} was not found.")
    { }
}
