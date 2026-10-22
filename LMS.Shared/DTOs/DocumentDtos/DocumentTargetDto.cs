using System;
using System.Collections.Generic;
using System.Text;

namespace LMS.Shared.DTOs.DocumentDtos;
public record DocumentTargetDto( //Ensures a readble label
    DocumentTarget Target,
    string Label);