using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Models.Exceptions;
    public sealed class ModuleNotFoundException : NotFoundException
    {
        public ModuleNotFoundException(int moduleId)
            : base($"Module with ID {moduleId} was not found.", "Module not found", "ModuleNotFound")
        {
        }
    }
