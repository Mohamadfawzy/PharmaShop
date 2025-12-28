using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contracts.Images.Dtos;


/// <summary>
/// Production DTO returned to API callers after a successful image save or replace.
/// This DTO is part of the public API contract and should remain backward-compatible.
/// </summary>
public sealed class SavedImageResult
{
    /// <summary>
    /// Stable unique identifier of the image.
    /// This value is stored in the database and reused for replace operations.
    /// </summary>
    public string Id { get; init; } = default!;

    /// <summary>
    /// Actual stored format of the image (Jpeg, Png, Webp).
    /// </summary>
    public StoredImageFormat Format { get; init; }

    /// <summary>
    /// Width of the original uploaded image (before resizing).
    /// </summary>
    public int OriginalWidth { get; init; }

    /// <summary>
    /// Height of the original uploaded image (before resizing).
    /// </summary>
    public int OriginalHeight { get; init; }

    /// <summary>
    /// UTC timestamp when the image was successfully saved.
    /// </summary>
    public DateTimeOffset SavedAtUtc { get; init; }

    /// <summary>
    /// Relative file paths for each image variant (Original, Medium, Small).
    /// These paths are relative to the image root folder.
    /// </summary>
    public IReadOnlyDictionary<ImageVariant, string> RelativePaths { get; init; }
        = new Dictionary<ImageVariant, string>();
}
