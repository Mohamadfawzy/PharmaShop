using Contracts.Images.Abstractions;
using Contracts.Images.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Constants;

namespace WebAPI.Controllers;
[ApiController]
[Authorize(Policy = AppPolicies.Admin)]
[ApiExplorerSettings(IgnoreApi = true)]
[Route("api/dev/images")]
public sealed class DevImagesController : ControllerBase
{
    private readonly IImageService _images;
    private readonly IWebHostEnvironment _env;

    public DevImagesController(IImageService images, IWebHostEnvironment env)
    {
        _images = images;
        _env = env;
    }

    private string RootPath => Path.Combine(_env.ContentRootPath, "wwwroot", "uploads");
    private bool IsDevelopment => _env.IsDevelopment();
    private static readonly HashSet<string> AllowedImageTypes = new(StringComparer.OrdinalIgnoreCase)
        { "image/jpeg", "image/png", "image/webp" };
    private static bool IsValidFile(IFormFile? file)
        => file is { Length: > 0 and <= 10 * 1024 * 1024 } && AllowedImageTypes.Contains(file.ContentType);
    private static bool IsSafeImageId(string? imageId)
        => !string.IsNullOrWhiteSpace(imageId) && imageId.Length <= 100
           && imageId.All(c => char.IsAsciiLetterOrDigit(c) || c is '-' or '_');

    // (1) Save
    [HttpPost("save")]
    [RequestSizeLimit(11 * 1024 * 1024)]
    public async Task<ActionResult<SavedImageResult>> Save(
        IFormFile file,
        [FromQuery] string? prefix,
        [FromQuery] ImageOutputFormat format = ImageOutputFormat.Auto,
        CancellationToken ct = default)
    {
        if (!IsDevelopment) return NotFound();
        if (!IsValidFile(file)) return BadRequest("Upload a JPEG, PNG, or WebP image no larger than 10 MB.");

        await using var stream = file.OpenReadStream();
        var result = await _images.SaveAsync(stream, RootPath, prefix, format, ct);

        return Ok(result);
    }

    // (2) Replace
    [HttpPut("{imageId}/replace")]
    [RequestSizeLimit(11 * 1024 * 1024)]
    public async Task<ActionResult<SavedImageResult>> Replace(
        string imageId,
        IFormFile file,
        [FromQuery] ImageOutputFormat format = ImageOutputFormat.Auto,
        CancellationToken ct = default)
    {
        if (!IsDevelopment) return NotFound();
        if (!IsSafeImageId(imageId))
            return BadRequest("Invalid imageId.");

        if (!IsValidFile(file)) return BadRequest("Upload a JPEG, PNG, or WebP image no larger than 10 MB.");

        await using var stream = file.OpenReadStream();
        var result = await _images.ReplaceAsync(stream, imageId, RootPath, format, ct);

        return Ok(result);
    }

    // (3) Delete
    [HttpDelete("{imageId}")]
    public async Task<IActionResult> Delete(string imageId, CancellationToken ct)
    {
        if (!IsDevelopment) return NotFound();
        if (!IsSafeImageId(imageId))
            return BadRequest("Invalid imageId.");

        await _images.DeleteAsync(imageId, RootPath, ct);
        return NoContent();
    }

    // (6) Batch save
    [HttpPost("batch")]
    [RequestSizeLimit(26 * 1024 * 1024)]
    public async Task<ActionResult<IReadOnlyList<SavedImageResult>>> Batch(
        [FromForm] List<IFormFile> files,
        [FromQuery] string? prefix,
        [FromQuery] ImageOutputFormat format = ImageOutputFormat.Auto,
        CancellationToken ct = default)
    {
        if (!IsDevelopment) return NotFound();
        if (files is null || files.Count == 0)
            return BadRequest("No files.");
        if (files.Count > 5 || files.Any(f => !IsValidFile(f)) || files.Sum(f => f.Length) > 25L * 1024 * 1024)
            return BadRequest("Upload up to 5 JPEG, PNG, or WebP images, each <= 10 MB and total <= 25 MB.");

        // Convert to streams (be careful: keep streams alive during call).
        var streams = new List<Stream>(files.Count);
        try
        {
            foreach (var f in files)
            {
                if (f.Length == 0) return BadRequest("One of the files is empty.");
                streams.Add(f.OpenReadStream());
            }

            var results = await _images.SaveBatchAsync(streams, RootPath, prefix, format, ct);
            return Ok(results);
        }
        finally
        {
            foreach (var s in streams) await s.DisposeAsync();
        }
    }

    // (5) Download variant (for visual verification)
    [HttpGet("{imageId}/{variant}")]
    public IActionResult DownloadVariant(
        string imageId,
        string variant,
        [FromQuery] string formatHint = "webp")
    {
        if (!IsDevelopment) return NotFound();
        if (!IsSafeImageId(imageId))
            return BadRequest("Invalid imageId.");

        var folder = variant.ToLowerInvariant() switch
        {
            "original" => "original",
            "medium" => "medium",
            "small" => "small",
            _ => null
        };
        if (folder is null) return BadRequest("Invalid variant. Use original|medium|small");

        var ext = formatHint.ToLowerInvariant() switch
        {
            "jpg" or "jpeg" => ".jpg",
            "png" => ".png",
            "webp" => ".webp",
            _ => ".webp"
        };

        var abs = Path.Combine(RootPath, folder, imageId + ext);
        if (!System.IO.File.Exists(abs))
            return NotFound($"File not found: {folder}/{imageId}{ext}");

        var contentType = ext switch
        {
            ".jpg" => "image/jpeg",
            ".png" => "image/png",
            ".webp" => "image/webp",
            _ => "application/octet-stream"
        };

        return PhysicalFile(abs, contentType);
    }
}
