using Contracts.Images.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contracts.Images.Abstractions;


/// <summary>
/// High-level application service: orchestrates validation, processing, storage and atomicity.
/// </summary>
public interface IImageService
{
    Task<SavedImageResult> SaveAsync(
        Stream imageData,
        string rootPath,
        string? prefix = null,
        CancellationToken ct = default);

    Task<IReadOnlyList<SavedImageResult>> SaveBatchAsync(
        IEnumerable<Stream> imagesData,
        string rootPath,
        string? prefix = null,
        CancellationToken ct = default);

    /// <summary>
    /// Replace an existing image contents while keeping the same imageId.
    /// This method is safe: it stages new files then swaps atomically.
    /// </summary>
    Task<SavedImageResult> ReplaceAsync(
        Stream imageData,
        string imageId,
        string rootPath,
        CancellationToken ct = default);

    Task DeleteAsync(string imageId, string rootPath, CancellationToken ct = default);
}