using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Models.Exceptions.DocumentExceptions
{
    public sealed class DocumentNotFoundException : NotFoundException
    {
        public DocumentNotFoundException(string message) : base(message) 
        { }
    }
}
