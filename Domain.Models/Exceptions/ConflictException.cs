using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Models.Exceptions;
public abstract class ConflictException : Exception
{
    public string Code { get; }

    protected ConflictException(string message, string code) : base(message)
    {
        Code = code;
    }
}
