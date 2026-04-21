using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Models.ImageDtos;


public sealed class UploadStreamFile
{
    public required Stream Content { get; init; }
    public string? FileName { get; init; }
    public string? ContentType { get; init; }
    public long? Length { get; init; }
}
