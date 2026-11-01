using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Models.Exceptions.DocumentExceptions;
    public sealed class DocumentAccessException : ForbiddenException
    {
        public DocumentAccessException() : base("You do not have permission to access this material.")
        {
        }
    }
