using Contracts.Images.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contracts.Images.Dtos;



/// <summary>
/// Internal structure returned by the processor.
/// Contains enough info to store the image safely.
/// </summary>
public sealed class ProcessedImage
{
    public required StoredImageFormat Format { get; init; }
    public required int OriginalWidth { get; init; }
    public required int OriginalHeight { get; init; }

    /// <summary>
    /// Encoded bytes per variant (already stripped from sensitive metadata).
    /// </summary>
    public required IReadOnlyDictionary<ImageVariant, byte[]> VariantBytes { get; init; }
}