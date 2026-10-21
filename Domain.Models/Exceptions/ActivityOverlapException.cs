using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Models.Exceptions;

public sealed class ActivityOverlapException : ConflictException
{
    public ActivityOverlapException() : base("The activity overlaps another activity in the same module")
    { }
}
