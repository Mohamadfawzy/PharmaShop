using Contracts.IServices;
using Microsoft.Extensions.Logging;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Processing;

namespace Service;
/// <summary>
///
/// </summary>
public class MyImageService : IMyImageService
{
    private const int MediumSize = 1000;
    private const int SmallSize = 300;
    private const string Original = "original";
    private const string Medium = "medium";
    private const string Small = "small";
    private const long MaxFileSizeBytes = 10 * 1024 * 1024; // 10 MB
    private readonly ILogger<MyImageService> _logger;

    public MyImageService(ILogger<MyImageService> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Implementation of SaveImageAsync: 
    /// creates small, medium, and original copies, 
    /// and stores them on disk under the given root path.
    /// </summary>
    public async Task<string> SaveImageAsync(Stream imageData, string rootPath, string prefix = "", CancellationToken ct = default)
    {
        try
        {
            ValidateInputs(imageData, rootPath);

            // Generate a unique GUID for the image
            var guid = Guid.NewGuid().ToString("N");
            var fileName = $"{prefix}-{guid}";
            var fileNameWithExtention = fileName + ".jpg";
            _logger.LogInformation("Start saving image with ID: {ImageId}", fileName);

            // Validate and load the image
            imageData.Position = 0;
            using var image = await LoadAndValidateImageAsync(imageData, ct);

            // Save multiple versions with the same ID
            await SaveResizedCopyAsync(image, fileNameWithExtention, Path.Combine(rootPath, Original), image.Width, 85, ct);
            await SaveResizedCopyAsync(image, fileNameWithExtention, Path.Combine(rootPath, Medium), MediumSize, 75, ct);
            await SaveResizedCopyAsync(image, fileNameWithExtention, Path.Combine(rootPath, Small), SmallSize, 70, ct);

            _logger.LogInformation("Image saved successfully with ID: {ImageId}", fileName);
            return fileName; // Return only the ID without extension
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save image");
            throw;
        }
    }

    /// <summary>
    /// Save a batch of images (each saved in 3 sizes) and return their generated IDs.
    /// If any image fails, previously saved images in this batch are rolled back (deleted).
    /// </summary>
    public async Task<List<string>> SaveMultipleImagesAsync(IEnumerable<Stream> imagesData, string rootPath, CancellationToken ct = default)
    {
        if (imagesData == null)
            throw new ArgumentNullException(nameof(imagesData));

        if (string.IsNullOrWhiteSpace(rootPath))
            throw new ArgumentException("Root path is required.", nameof(rootPath));

        var savedIds = new List<string>();
        int index = 0;

        try
        {
            foreach (var imageStream in imagesData)
            {
                ct.ThrowIfCancellationRequested();

                // Basic validation for the current stream before passing it down
                if (imageStream == null || !imageStream.CanRead || imageStream.Length == 0)
                    throw new ArgumentException($"Image at index {index} is invalid or empty.", nameof(imagesData));

                // Ensure position is at start if seekable
                if (imageStream.CanSeek) imageStream.Position = 0;

                _logger.LogInformation("Saving image {Current}/{TotalOrUnknown}", index + 1, "?");

                // Reuse the single-image pipeline (validations + 3-size save + logging)
                var imageId = await SaveImageAsync(imageStream, rootPath: rootPath, ct: ct);

                savedIds.Add(imageId);
                _logger.LogInformation("Image {Index} saved successfully with ID: {ImageId}", index, imageId);

                index++;
            }

            _logger.LogInformation("Batch saved successfully. Total images: {Count}", savedIds.Count);
            return savedIds;
        }
        catch (Exception ex)
        {
            // Rollback previously saved images in this batch to keep the operation atomic
            _logger.LogError(ex, "Failed to save images batch. Rolling back {Count} saved images.", savedIds.Count);

            foreach (var id in savedIds)
            {
                try
                {
                    RemoveImageAsync(id, rootPath);
                    _logger.LogWarning("Rolled back image with ID: {ImageId}", id);
                }
                catch (Exception rollbackEx)
                {
                    // Best-effort rollback — we log and continue
                    _logger.LogWarning(rollbackEx, "Rollback failed for image ID: {ImageId}", id);
                }
            }

            throw; // rethrow to let the caller handle the failure
        }
    }

    /// <summary>
    /// Update an existing image by replacing it with a new one
    /// </summary>
    public async Task<string> ReplaceImageAsync(Stream imageData, string imageId, string rootPath, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Start updating image: {ImageId}", imageId);
            RemoveImageAsync(imageId, rootPath);
            return await SaveImageAsync(imageData, rootPath, ct: ct); // empty string since filename is not used
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update image: {ImageId}", imageId);
            throw;
        }
    }

    /// <summary>
    /// Remove an image (all sizes) by its ID
    /// </summary>
    public Task RemoveImageAsync(string imageId, string rootPath)
    {
        try
        {
            var fileName = imageId + ".jpg";
            _logger.LogInformation("Start deleting image: {ImageId}", imageId);

            var results = new[]
            {
                DeleteIfExists(Path.Combine(rootPath, Original, fileName)),
                DeleteIfExists(Path.Combine(rootPath, Medium, fileName)),
                DeleteIfExists(Path.Combine(rootPath, Small, fileName))
            };

            var deletedCount = results.Count(r => r);
            _logger.LogInformation("Deleted {Count} files for image: {ImageId}", deletedCount, imageId);

            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete image: {ImageId}", imageId);
            throw;
        }
    }

    /// <summary>
    /// Validate input stream and root path
    /// </summary>
    private static void ValidateInputs(Stream imageData, string rootPath)
    {
        if (imageData == null || imageData.Length == 0)
            throw new ArgumentException("Image is invalid or empty.", nameof(imageData));

        if (string.IsNullOrWhiteSpace(rootPath))
            throw new ArgumentException("Root path is required.", nameof(rootPath));

        if (imageData.Length > MaxFileSizeBytes)
            throw new ArgumentException($"Image size is too large. Max allowed: {MaxFileSizeBytes / (1024 * 1024)} MB", nameof(imageData));
    }

    /// <summary>
    /// Load and validate image dimensions and format
    /// </summary>
    private static async Task<Image> LoadAndValidateImageAsync(Stream imageData, CancellationToken ct)
    {
        try
        {
            var image = await Image.LoadAsync(imageData, ct);

            // Validate reasonable dimensions
            if (image.Width < 1 || image.Height < 1 || image.Width > 50000 || image.Height > 50000)
            {
                image.Dispose();
                throw new ArgumentException("Image dimensions are invalid.");
            }

            return image;
        }
        catch (UnknownImageFormatException)
        {
            throw new ArgumentException("Unsupported image format.");
        }
        catch (InvalidImageContentException)
        {
            throw new ArgumentException("Corrupted or invalid image content.");
        }
    }

    /// <summary>
    /// Save a resized copy of the image to disk
    /// </summary>
    private async Task SaveResizedCopyAsync(Image original, string fileNameJpg, string folderPath, int targetWidth, int jpegQuality, CancellationToken ct)
    {
        try
        {
            Directory.CreateDirectory(folderPath);
            var destPath = Path.Combine(folderPath, fileNameJpg);

            int width = original.Width;
            int height = original.Height;

            // Do not upscale — only resize if the image is larger than targetWidth
            if (width > targetWidth)
            {
                var ratio = (double)targetWidth / width;
                width = targetWidth;
                height = (int)Math.Round(height * ratio);
            }

            // Always work on a clone and apply ResizeOptions with Lanczos3
            using var resized = original.Clone(ctx => ctx.Resize(new ResizeOptions
            {
                Size = new Size(width, height),
                Sampler = KnownResamplers.Lanczos3,
                Mode = ResizeMode.Max
            }));

            // Remove sensitive metadata
            resized.Metadata.ExifProfile = null;
            resized.Metadata.IccProfile = null;
            resized.Metadata.XmpProfile = null;

            var encoder = new JpegEncoder { Quality = jpegQuality };
            await resized.SaveAsJpegAsync(destPath, encoder, ct);

            _logger.LogDebug("Saved resized copy: {Path} ({Width}x{Height})", destPath, width, height);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save resized copy in: {FolderPath}", folderPath);
            throw;
        }
    }

    /// <summary>
    /// Delete image file if it exists
    /// </summary>
    private bool DeleteIfExists(string filePath)
    {
        try
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
                _logger.LogDebug("Deleted file: {FilePath}", filePath);
                return true;
            }
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to delete file: {FilePath}", filePath);
            return false;
        }
    }
}
