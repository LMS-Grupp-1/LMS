using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Models.Exceptions;
    public enum ActivityValidationError
    {
        InvalidName,
        InvalidTimeRange,
        ModuleNotFound,
        OutsideModulePeriod,
        Overlap
    }

    public sealed class ActivityValidationException : Exception
    {
        public ActivityValidationError ErrorCode { get; }
        public ActivityValidationException(
            ActivityValidationError errorCode,
            string message) : base(message)
        {  ErrorCode = errorCode; }
    }
