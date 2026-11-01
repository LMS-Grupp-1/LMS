using System;
using System.Collections.Generic;
using System.Text;

namespace LMS.Shared.DTOs.Users;

public record UserLookupDto(
    string Id,
    string Name
);
