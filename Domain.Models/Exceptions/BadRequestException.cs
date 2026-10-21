using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Models.Exceptions;
    public abstract class BadRequestException : Exception
    {
        protected BadRequestException(string message) : base(message) 
        {
        }
    }
