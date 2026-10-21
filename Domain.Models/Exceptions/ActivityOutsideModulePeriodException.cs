using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Models.Exceptions;

    public sealed class ActivityOutsideModulePeriodException : BadRequestException
{
    public ActivityOutsideModulePeriodException() : base(
        "The activity must be within the module's date range.")
    { }
}