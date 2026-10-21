using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Models.Exceptions;
public abstract class ConflictException : Exception
{
    protected ConflictException(string message) : base(message)
    {
    }
}
