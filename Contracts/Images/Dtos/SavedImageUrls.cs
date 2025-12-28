using Contracts.Images.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contracts.Images.Dtos;

///<summary>
/// DTO optimized for client/UI consumption.
/// Contains resolved public URLs instead of file system paths.
/// </summary>
public sealed class SavedImageUrls
{
    /// <summary>
    /// Image identifier (same value stored in the database).
    /// </summary>
    public string Id { get; init; } = default!;

    /// <summary>
    /// Public URLs for each image variant (Original, Medium, Small).
    /// Example: https://cdn.example.com/images/medium/abc123.webp
    /// </summary>
    public IReadOnlyDictionary<ImageVariant, string> Urls { get; init; }
        = new Dictionary<ImageVariant, string>();
}