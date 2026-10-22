using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Models.Documents;
    public record DocumentActor(
    string UserId,
    bool IsTeacher,
    bool IsStudent);