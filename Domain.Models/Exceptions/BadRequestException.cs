using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Models.Exceptions;
    public abstract class BadRequestException : Exception
    {
        public string Code { get; }

        protected BadRequestException(string message, string code) : base(message) 
        {
            Code = code;
        }
    }
