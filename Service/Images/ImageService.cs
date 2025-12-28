using Contracts.Images.Abstractions;
using Contracts.Images.Dtos;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Images;


public sealed class ImageService : IImageService
{
    private readonly ILogger<ImageService> _logger;
    private readonly IImageProcessor _processor;
    private readonly IImageStorage _storage;
    private readonly Contracts.Images.Dtos.ImageServiceOptions _opt;

    // IMPORTANT: per-image lock to prevent race conditions on Replace/Delete/Save with same id.
    private static readonly ConcurrentDictionary<string, SemaphoreSlim> _locks = new();

    public ImageService(
        ILogger<ImageService> logger,
        IImageProcessor processor,
        IImageStorage storage,
        IOptions<Contracts.Images.Dtos.ImageServiceOptions> options)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _processor = processor ?? throw new ArgumentNullException(nameof(processor));
        _storage = storage ?? throw new ArgumentNullException(nameof(storage));
        _opt = options?.Value ?? throw new ArgumentNullException(nameof(options));
    }

    public async Task<SavedImageResult> SaveAsync(
        Stream imageData,
        string rootPath,
        string? prefix = null,
        CancellationToken ct = default)
    {
        if (imageData is null) throw new ArgumentNullException(nameof(imageData));
        if (string.IsNullOrWhiteSpace(rootPath)) throw new ArgumentException("Root path is required.", nameof(rootPath));

        // IMPORTANT: sanitize prefix early (never trust input).
        var safePrefix = SanitizePrefix(prefix);

        // Generate a stable unique id.
        var imageId = safePrefix is null
            ? Guid.NewGuid().ToString("N")
            : $"{safePrefix}-{Guid.NewGuid():N}";

        return await SaveWithIdInternalAsync(imageData, imageId, rootPath, allowOverwrite: false, ct);
    }

    public async Task<IReadOnlyList<SavedImageResult>> SaveBatchAsync(
        IEnumerable<Stream> imagesData,
        string rootPath,
        string? prefix = null,
        CancellationToken ct = default)
    {
        if (imagesData is null) throw new ArgumentNullException(nameof(imagesData));
        if (string.IsNullOrWhiteSpace(rootPath)) throw new ArgumentException("Root path is required.", nameof(rootPath));

        // Optional: if collection is countable, log it.
        var list = imagesData as IList<Stream> ?? imagesData.ToList();
        var total = list.Count;

        using var scope = _logger.BeginScope(new Dictionary<string, object>
        {
            ["BatchId"] = Guid.NewGuid().ToString("N"),
            ["Total"] = total
        });

        var results = new List<SavedImageResult>(total);

        // IMPORTANT: Make batch atomic at service-level:
        // - If any fails, delete previously saved images (best-effort).
        try
        {
            for (var i = 0; i < total; i++)
            {
                ct.ThrowIfCancellationRequested();

                _logger.LogInformation("Saving batch image {Index}/{Total}", i + 1, total);

                var res = await SaveAsync(list[i], rootPath, prefix, ct);
                results.Add(res);
            }

            return results;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Batch save failed. Rolling back {Count} images.", results.Count);

            foreach (var r in results)
            {
                try
                {
                    await DeleteAsync(r.Id, rootPath, ct);
                    _logger.LogWarning("Rolled back image {ImageId}", r.Id);
                }
                catch (Exception rollbackEx)
                {
                    // Best-effort rollback
                    _logger.LogWarning(rollbackEx, "Rollback failed for image {ImageId}", r.Id);
                }
            }

            throw;
        }
    }

    public async Task<SavedImageResult> ReplaceAsync(
        Stream imageData,
        string imageId,
        string rootPath,
        CancellationToken ct = default)
    {
        if (imageData is null) throw new ArgumentNullException(nameof(imageData));
        if (string.IsNullOrWhiteSpace(imageId)) throw new ArgumentException("ImageId is required.", nameof(imageId));
        if (string.IsNullOrWhiteSpace(rootPath)) throw new ArgumentException("Root path is required.", nameof(rootPath));

        // IMPORTANT: lock by id to avoid races with delete/other replaces.
        var gate = GetLock(imageId);
        await gate.WaitAsync(ct);
        try
        {
            _logger.LogInformation("Replacing image {ImageId}", imageId);

            // Stage & overwrite atomically via storage.
            // We do NOT delete old first. The atomic save will overwrite final files.
            var result = await SaveWithIdInternalAsync(imageData, imageId, rootPath, allowOverwrite: true, ct);
            return result;
        }
        finally
        {
            gate.Release();
        }
    }

    public async Task DeleteAsync(string imageId, string rootPath, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(imageId)) throw new ArgumentException("ImageId is required.", nameof(imageId));
        if (string.IsNullOrWhiteSpace(rootPath)) throw new ArgumentException("Root path is required.", nameof(rootPath));

        var gate = GetLock(imageId);
        await gate.WaitAsync(ct);
        try
        {
            _logger.LogInformation("Deleting image {ImageId}", imageId);

            // We don't know the format, so storage deletes all possible extensions.
            await _storage.DeleteAllAsync(rootPath, imageId, formatHint: null, ct);
        }
        finally
        {
            gate.Release();
        }
    }

    // ==========================
    // Internal save pipeline
    // ==========================

    private async Task<SavedImageResult> SaveWithIdInternalAsync(
        Stream imageData,
        string imageId,
        string rootPath,
        bool allowOverwrite,
        CancellationToken ct)
    {
        // IMPORTANT: Buffer stream safely (supports non-seekable streams)
        // and enforce maximum size to avoid memory explosions.
        await using var buffered = await BufferToMemoryAsync(imageData, _opt.MaxUploadBytes, ct);

        using var scope = _logger.BeginScope(new Dictionary<string, object>
        {
            ["ImageId"] = imageId
        });

        _logger.LogInformation("Processing image {ImageId}", imageId);

        // Decode + validate + create variants
        var processed = await _processor.ProcessAsync(buffered, ct);

        // Build storage instructions (relative paths)
        var files = new List<ImageFileToSave>(3);
        foreach (var kv in processed.VariantBytes)
        {
            var rel = _storage.BuildRelativePath(imageId, kv.Key, processed.Format);
            files.Add(new ImageFileToSave(kv.Key, rel, kv.Value));
        }

        // IMPORTANT: If not allowing overwrite and files exist, you can guard here.
        // However, since we use GUID for new IDs, collisions are virtually impossible.
        // For Replace, allowOverwrite=true.

        _logger.LogInformation("Saving image variants atomically for {ImageId}", imageId);
        await _storage.SaveVariantsAtomicallyAsync(rootPath, imageId, processed.Format, files, ct);

        var relativePaths = files.ToDictionary(f => f.Variant, f => f.RelativePath);

        _logger.LogInformation("Image {ImageId} saved successfully", imageId);

        return new SavedImageResult(
            Id: imageId,
            Format: processed.Format,
            OriginalWidth: processed.OriginalWidth,
            OriginalHeight: processed.OriginalHeight,
            SavedAtUtc: DateTimeOffset.UtcNow,
            RelativePaths: relativePaths);
    }

    // ==========================
    // Helpers
    // ==========================

    private static SemaphoreSlim GetLock(string id)
        => _locks.GetOrAdd(id, _ => new SemaphoreSlim(1, 1));

    private string? SanitizePrefix(string? prefix)
    {
        if (string.IsNullOrWhiteSpace(prefix))
            return null;

        var trimmed = prefix.Trim();

        // IMPORTANT: keep only safe chars (letters/digits/_/-). Prevent path injection.
        var safe = new string(trimmed
            .Take(_opt.MaxPrefixLength)
            .Select(ch => char.IsLetterOrDigit(ch) ? ch : (ch == '-' || ch == '_' ? ch : '\0'))
            .Where(ch => ch != '\0')
            .ToArray());

        return string.IsNullOrWhiteSpace(safe) ? null : safe;
    }

    private static async Task<MemoryStream> BufferToMemoryAsync(Stream input, long maxBytes, CancellationToken ct)
    {
        // IMPORTANT: Copy to memory with a hard max size.
        // If your files may exceed memory, use a temp file buffer strategy instead.
        if (maxBytes <= 0) throw new ArgumentOutOfRangeException(nameof(maxBytes));

        var ms = new MemoryStream(capacity: 256 * 1024);
        var buffer = new byte[64 * 1024];

        long total = 0;
        while (true)
        {
            ct.ThrowIfCancellationRequested();

            var read = await input.ReadAsync(buffer.AsMemory(0, buffer.Length), ct);
            if (read == 0) break;

            total += read;
            if (total > maxBytes)
                throw new ArgumentException($"Image size is too large. Max allowed: {maxBytes / (1024 * 1024)} MB");

            await ms.WriteAsync(buffer.AsMemory(0, read), ct);
        }

        ms.Position = 0;
        return ms;
    }
}