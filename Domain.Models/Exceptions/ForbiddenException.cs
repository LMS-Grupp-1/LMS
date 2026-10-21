using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Models.Exceptions;
    public abstract class ForbiddenException : Exception
    {
    protected ForbiddenException(string message) : base(message)
    { }
    }
