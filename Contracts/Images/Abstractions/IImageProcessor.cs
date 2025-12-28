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
    Task<ProcessedImage> ProcessAsync(Stream input, CancellationToken ct);
}

/// <summary>
/// Responsible for file-system storage + atomic write semantics.
/// Can be replaced with S3/Azure Blob later.
/// </summary>
public interface IImageStorage
{
    Task SaveVariantsAtomicallyAsync(
        string rootPath,
        string imageId,
        StoredImageFormat format,
        IReadOnlyList<ImageFileToSave> files,
        CancellationToken ct);

    Task DeleteAllAsync(string rootPath, string imageId, StoredImageFormat? formatHint, CancellationToken ct);

    /// <summary>
    /// Builds a relative path (used by APIs/DB).
    /// </summary>
    string BuildRelativePath(string imageId, ImageVariant variant, StoredImageFormat format);
}