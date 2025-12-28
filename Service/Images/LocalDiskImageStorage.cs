using Contracts.Images.Abstractions;
using Contracts.Images.Dtos;
using Contracts.Images.Enums;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Images;

public sealed class LocalDiskImageStorage : IImageStorage
{
    private readonly Contracts.Images.Dtos.ImageServiceOptions _opt;

    public LocalDiskImageStorage(IOptions<Contracts.Images.Dtos.ImageServiceOptions> options)
    {
        _opt = options?.Value ?? throw new ArgumentNullException(nameof(options));
    }

    public string BuildRelativePath(string imageId, ImageVariant variant, StoredImageFormat format)
    {
        var folder = variant switch
        {
            ImageVariant.Original => _opt.OriginalFolder,
            ImageVariant.Medium => _opt.MediumFolder,
            ImageVariant.Small => _opt.SmallFolder,
            _ => throw new ArgumentOutOfRangeException(nameof(variant))
        };

        var ext = format switch
        {
            StoredImageFormat.Jpeg => ".jpg",
            StoredImageFormat.Png => ".png",
            StoredImageFormat.Webp => ".webp",
            _ => throw new ArgumentOutOfRangeException(nameof(format))
        };

        return Path.Combine(folder, imageId + ext);
    }

    public async Task SaveVariantsAtomicallyAsync(
        string rootPath,
        string imageId,
        StoredImageFormat format,
        IReadOnlyList<ImageFileToSave> files,
        CancellationToken ct)
    {
        // IMPORTANT: Atomic strategy:
        // - Write every file to temp path
        // - If all succeed: move temp -> final (replace)
        // - If anything fails: delete temp files (rollback)

        EnsureRoot(rootPath);

        var tmpRoot = Path.Combine(rootPath, _opt.TempFolderName, imageId);
        Directory.CreateDirectory(tmpRoot);

        var tempPaths = new List<(string Temp, string Final)>();

        try
        {
            foreach (var f in files)
            {
                ct.ThrowIfCancellationRequested();

                var finalAbsolute = Path.Combine(rootPath, f.RelativePath);

                // Ensure final directory exists
                Directory.CreateDirectory(Path.GetDirectoryName(finalAbsolute)!);

                // Build temp absolute path per variant
                var tempAbsolute = Path.Combine(tmpRoot, Path.GetFileName(finalAbsolute) + ".tmp");

                // Write temp
                await File.WriteAllBytesAsync(tempAbsolute, f.Content, ct);

                tempPaths.Add((tempAbsolute, finalAbsolute));
            }

            // Swap: Move temp files to final paths (overwrite if exists).
            // On Windows, File.Move doesn't overwrite by default: use File.Copy + Replace logic.
            foreach (var (temp, final) in tempPaths)
            {
                ct.ThrowIfCancellationRequested();
                ReplaceFile(temp, final);
            }
        }
        catch
        {
            // Best-effort cleanup temp
            SafeDeleteDirectory(tmpRoot);
            throw;
        }
        finally
        {
            // Cleanup temp directory (if empty or after move)
            SafeDeleteDirectory(tmpRoot);
        }
    }

    public async Task DeleteAllAsync(string rootPath, string imageId, StoredImageFormat? formatHint, CancellationToken ct)
    {
        EnsureRoot(rootPath);

        // If format is unknown, delete all possible extensions to be safe.
        var formats = formatHint.HasValue
            ? new[] { formatHint.Value }
            : new[] { StoredImageFormat.Jpeg, StoredImageFormat.Png, StoredImageFormat.Webp };

        foreach (var fmt in formats)
        {
            ct.ThrowIfCancellationRequested();

            foreach (var variant in new[] { ImageVariant.Original, ImageVariant.Medium, ImageVariant.Small })
            {
                var rel = BuildRelativePath(imageId, variant, fmt);
                var abs = Path.Combine(rootPath, rel);

                // Best-effort delete
                if (File.Exists(abs))
                {
                    try { File.Delete(abs); }
                    catch { /* swallow here; the service decides strictness */ }
                }
            }
        }

        await Task.CompletedTask;
    }

    private static void EnsureRoot(string rootPath)
    {
        if (string.IsNullOrWhiteSpace(rootPath))
            throw new ArgumentException("Root path is required.", nameof(rootPath));

        Directory.CreateDirectory(rootPath);
    }

    private static void ReplaceFile(string tempPath, string finalPath)
    {
        // IMPORTANT: Replace semantics:
        // - If final exists, replace it.
        // - Then delete temp file.
        if (File.Exists(finalPath))
        {
            // File.Replace works best when both files exist.
            // We'll do a robust approach: copy temp -> final (overwrite), then delete temp.
            File.Copy(tempPath, finalPath, overwrite: true);
            File.Delete(tempPath);
            return;
        }

        File.Move(tempPath, finalPath);
    }

    private static void SafeDeleteDirectory(string dir)
    {
        try
        {
            if (Directory.Exists(dir))
                Directory.Delete(dir, recursive: true);
        }
        catch
        {
            // Best-effort only
        }
    }
}