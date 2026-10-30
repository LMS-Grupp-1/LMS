using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Models.Exceptions.ActivityExceptions;

    public sealed class ActivityOutsideModulePeriodException : BadRequestException
{
    public ActivityOutsideModulePeriodException() : base(
        "The activity must be within the module's date range.")
    { }
}