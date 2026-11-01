using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Models.Exceptions.DocumentExceptions
{
    public sealed class DocumentValidationException : BadRequestException
    {
        public DocumentValidationException(string message) : base(message) 
        { }
    }
}
