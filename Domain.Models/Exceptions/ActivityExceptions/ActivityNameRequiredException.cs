using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Models.Exceptions.ActivityExceptions;

public sealed class ActivityNameRequiredException : BadRequestException
{
    public ActivityNameRequiredException() : base("Activity name is required.")
    { }
}