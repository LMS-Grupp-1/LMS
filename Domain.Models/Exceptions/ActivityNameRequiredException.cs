using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Models.Exceptions;

public sealed class ActivityNameRequiredException : BadRequestException
{
    public ActivityNameRequiredException() : base("Activity name is required.", "ActivityNameRequired")
    { }
}