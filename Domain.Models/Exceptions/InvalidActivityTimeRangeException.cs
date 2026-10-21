using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Models.Exceptions;

public sealed class InvalidActivityTimeRangeException : BadRequestException
{
    public InvalidActivityTimeRangeException() : base(
        "Activity start and end times are required. " +
        "and the start must be earlier than the end.",
        "InvalidActivityTimeRange")
    { }
}