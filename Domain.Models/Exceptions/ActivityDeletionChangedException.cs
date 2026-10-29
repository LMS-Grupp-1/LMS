using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Models.Exceptions;
    public sealed class ActivityDeletionChangedException : ConflictException
    {
        public ActivityDeletionChangedException()
            : base("The activity content has changed. Review the deletion preview again.")
        {
        }
    }
