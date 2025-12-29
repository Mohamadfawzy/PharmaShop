using Contracts.Images.Dtos;
using Contracts.Images.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contracts.Images.Abstractions;

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