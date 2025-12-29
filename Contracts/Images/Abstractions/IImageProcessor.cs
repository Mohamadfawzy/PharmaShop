using Contracts.Images.Dtos;
using Contracts.Images.Enums;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Contracts.Images.Abstractions;



/// <summary>
/// Responsible for decoding, validating and generating variants (resized/encoded bytes).
/// No filesystem code here.
/// </summary>
public interface IImageProcessor
{
    Task<ProcessedImage> ProcessAsync(
        Stream input, 
        ImageOutputFormat outputFormat,
        CancellationToken ct);
}

