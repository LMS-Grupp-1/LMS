using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Models.Documents;
public record DocumentDownload(
    Stream Content,
    string ContentType,
    string FileName);