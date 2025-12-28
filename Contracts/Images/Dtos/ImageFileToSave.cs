using Contracts.Images.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contracts.Images.Dtos;

/// <summary>
/// Internal model representing a single image variant
/// that should be persisted by the storage layer.
/// 
/// This class is NOT part of the public API contract.
/// It is used only internally between the processor
/// and the storage implementation.
/// </summary>
public sealed class ImageFileToSave
{
    /// <summary>
    /// Image variant type (Original, Medium, Small).
    /// </summary>
    public ImageVariant Variant { get; }

    /// <summary>
    /// Relative path where the image should be stored
    /// (e.g. "medium/abc123.webp").
    /// </summary>
    public string RelativePath { get; }

    /// <summary>
    /// Encoded binary content of the image variant.
    /// </summary>
    public byte[] Content { get; }

    /// <summary>
    /// Creates a new image file instruction for the storage layer.
    /// </summary>
    /// <param name="variant">Target image variant.</param>
    /// <param name="relativePath">Relative storage path.</param>
    /// <param name="content">Encoded image bytes.</param>
    public ImageFileToSave(
        ImageVariant variant,
        string relativePath,
        byte[] content)
    {
        Variant = variant;
        RelativePath = relativePath ?? throw new ArgumentNullException(nameof(relativePath));
        Content = content ?? throw new ArgumentNullException(nameof(content));
    }
}