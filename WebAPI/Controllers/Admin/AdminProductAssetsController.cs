using Contracts;
using Contracts.Images.Abstractions;
using Contracts.Images.Dtos;
using Entities.Models;
using Microsoft.AspNetCore.Mvc;
using Shared.Models.Dtos.Product;

namespace WebAPI.Controllers.Admin;

[Route("api/v1/admin/products/{productId:int}/images")]
public sealed class AdminProductAssetsController : AdminBaseApiController
{
    private static readonly HashSet<string> AllowedContentTypes = new(StringComparer.OrdinalIgnoreCase)
        { "image/jpeg", "image/png", "image/webp" };
    private const long MaxImageBytes = 10 * 1024 * 1024;
    private readonly IUnitOfWork _uow;
    private readonly IImageService _images;
    private readonly ILogger<AdminProductAssetsController> _logger;

    public AdminProductAssetsController(IUnitOfWork uow, IImageService images, IWebHostEnvironment env,
        ILogger<AdminProductAssetsController> logger) : base(env)
    { _uow = uow; _images = images; _logger = logger; }

    [HttpGet]
    public async Task<IActionResult> List(int productId, CancellationToken ct)
    {
        if (!await _uow.Products.AnyAsync(p => p.Id == productId && p.DeletedAt == null, ct)) return NotFound();
        var images = await _uow.ProductImages.GetAllAsync(criteria: x => x.ProductId == productId && x.DeletedAt == null,
            orderBy: q => q.OrderBy(x => x.SortOrder).ThenBy(x => x.Id), ct: ct);
        return Ok(images.Select(ToDto));
    }

    [HttpPost]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(11 * 1024 * 1024)]
    public async Task<IActionResult> Add(int productId, [FromForm] IFormFile image, [FromForm] string? altText,
        [FromForm] bool isPrimary = false, [FromQuery] ImageOutputFormat format = ImageOutputFormat.Auto,
        CancellationToken ct = default)
    {
        if (!IsValidImage(image)) return BadRequest("Upload a JPEG, PNG, or WebP image no larger than 10 MB.");
        if (altText?.Length > 200) return BadRequest("AltText cannot exceed 200 characters.");
        var product = await _uow.Products.GetByIdNoTrackingAsync(productId, ct);
        if (product is null || product.DeletedAt is not null) return NotFound("Product not found");
        var store = await _uow.Stores.GetByIdNoTrackingAsync(product.StoreId, ct);
        if (store is null) return BadRequest("Product store not found");
        var existingCount = await _uow.ProductImages.CountAsync(x => x.ProductId == productId && x.DeletedAt == null, ct);
        var primaryCount = await _uow.ProductImages.CountAsync(x => x.ProductId == productId && x.DeletedAt == null && x.IsPrimary, ct);
        isPrimary = isPrimary || existingCount == 0 || primaryCount == 0;
        await using var stream = image.OpenReadStream();
        SavedImageResult saved;
        try { saved = await _images.SaveAsync(stream, UploadsRootPath, "product", format, ct); }
        catch (ArgumentException ex) { return BadRequest(ex.Message); }

        await using var tx = await _uow.BeginTransactionAsync(ct);
        try
        {
            if (isPrimary)
            {
                var oldPrimary = await _uow.ProductImages.GetAllAsync(criteria: x => x.ProductId == productId && x.DeletedAt == null && x.IsPrimary,
                    asNoTracking: false, ct: ct);
                foreach (var old in oldPrimary) old.IsPrimary = false;
                if (oldPrimary.Count > 0) await _uow.CompleteAsync(ct);
            }
            var row = new ProductImage
            {
                PharmacyId = store.PharmacyId, ProductId = productId,
                ImageUrl = ToPublicUrl(saved, "original"), ThumbnailUrl = ToPublicUrl(saved, "small"),
                AltText = altText?.Trim(), SortOrder = existingCount, IsPrimary = isPrimary, CreatedAt = DateTime.UtcNow
            };
            await _uow.ProductImages.AddAsync(row, ct);
            await _uow.CompleteAsync(ct);
            await tx.CommitAsync(ct);
            return Ok(ToDto(row));
        }
        catch
        {
            await tx.RollbackAsync(ct);
            await TryDeleteAsync(saved.Id, ct);
            throw;
        }
    }

    [HttpPut("{imageId:long}")]
    public async Task<IActionResult> Update(int productId, long imageId, [FromBody] ProductImageUpdateDto dto, CancellationToken ct)
    {
        if (dto is null) return BadRequest("Request body is required.");
        if (dto.AltText?.Length > 200) return BadRequest("AltText cannot exceed 200 characters.");
        if (dto.SortOrder.HasValue && dto.SortOrder.Value < 0) return BadRequest("SortOrder cannot be negative.");
        var row = await _uow.ProductImages.FirstOrDefaultAsync(x => x.Id == imageId && x.ProductId == productId && x.DeletedAt == null,
            asNoTracking: false, ct: ct);
        if (row is null) return NotFound();
        await using var tx = await _uow.BeginTransactionAsync(ct);
        try
        {
            if (dto.IsPrimary == true)
            {
                var others = await _uow.ProductImages.GetAllAsync(criteria: x => x.ProductId == productId && x.Id != imageId && x.DeletedAt == null && x.IsPrimary,
                    asNoTracking: false, ct: ct);
                foreach (var other in others) other.IsPrimary = false;
                if (others.Count > 0) await _uow.CompleteAsync(ct);
                row.IsPrimary = true;
            }
            else if (dto.IsPrimary == false && row.IsPrimary)
            {
                row.IsPrimary = false;
                await _uow.ProductImages.UpdateAsync(row, ct);
                await _uow.CompleteAsync(ct);
                var replacement = await _uow.ProductImages.FirstOrDefaultAsync(x => x.ProductId == productId && x.Id != imageId && x.DeletedAt == null,
                    q => q.OrderBy(x => x.SortOrder).ThenBy(x => x.Id), asNoTracking: false, ct: ct);
                if (replacement is not null) replacement.IsPrimary = true;
            }
            if (dto.SortOrder.HasValue) row.SortOrder = dto.SortOrder.Value;
            if (dto.AltText is not null) row.AltText = dto.AltText.Trim();
            await _uow.ProductImages.UpdateAsync(row, ct);
            await _uow.CompleteAsync(ct);
            await tx.CommitAsync(ct);
            return Ok(ToDto(row));
        }
        catch { await tx.RollbackAsync(ct); throw; }
    }

    [HttpPut("{imageId:long}/file")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(11 * 1024 * 1024)]
    public async Task<IActionResult> ReplaceFile(int productId, long imageId, [FromForm] IFormFile image,
        [FromQuery] ImageOutputFormat format = ImageOutputFormat.Auto, CancellationToken ct = default)
    {
        if (!IsValidImage(image)) return BadRequest("Upload a JPEG, PNG, or WebP image no larger than 10 MB.");
        var row = await _uow.ProductImages.FirstOrDefaultAsync(x => x.Id == imageId && x.ProductId == productId && x.DeletedAt == null,
            asNoTracking: false, ct: ct);
        if (row is null) return NotFound();
        await using var stream = image.OpenReadStream();
        SavedImageResult saved;
        try { saved = await _images.SaveAsync(stream, UploadsRootPath, "product", format, ct); }
        catch (ArgumentException ex) { return BadRequest(ex.Message); }
        var oldImageId = GetStoredImageId(row.ImageUrl);
        await using var tx = await _uow.BeginTransactionAsync(ct);
        try
        {
            row.ImageUrl = ToPublicUrl(saved, "original");
            row.ThumbnailUrl = ToPublicUrl(saved, "small");
            await _uow.ProductImages.UpdateAsync(row, ct);
            await _uow.CompleteAsync(ct);
            await tx.CommitAsync(ct);
        }
        catch
        {
            await tx.RollbackAsync(ct);
            await TryDeleteAsync(saved.Id, ct);
            throw;
        }
        if (oldImageId is not null) await TryDeleteAsync(oldImageId, ct);
        return Ok(ToDto(row));
    }

    [HttpDelete("{imageId:long}")]
    public async Task<IActionResult> Delete(int productId, long imageId, CancellationToken ct)
    {
        var row = await _uow.ProductImages.FirstOrDefaultAsync(x => x.Id == imageId && x.ProductId == productId && x.DeletedAt == null,
            asNoTracking: false, ct: ct);
        if (row is null) return NotFound();
        var storedId = GetStoredImageId(row.ImageUrl);
        await using var tx = await _uow.BeginTransactionAsync(ct);
        try
        {
            var wasPrimary = row.IsPrimary;
            row.DeletedAt = DateTime.UtcNow; row.IsPrimary = false;
            await _uow.ProductImages.UpdateAsync(row, ct);
            await _uow.CompleteAsync(ct);
            if (wasPrimary)
            {
                var replacement = await _uow.ProductImages.FirstOrDefaultAsync(x => x.ProductId == productId && x.Id != imageId && x.DeletedAt == null,
                    q => q.OrderBy(x => x.SortOrder).ThenBy(x => x.Id), asNoTracking: false, ct: ct);
                if (replacement is not null) { replacement.IsPrimary = true; await _uow.ProductImages.UpdateAsync(replacement, ct); await _uow.CompleteAsync(ct); }
            }
            await tx.CommitAsync(ct);
        }
        catch { await tx.RollbackAsync(ct); throw; }
        if (storedId is not null) await TryDeleteAsync(storedId, ct);
        return NoContent();
    }

    private static bool IsValidImage(IFormFile? file)
        => file is { Length: > 0 and <= MaxImageBytes } && AllowedContentTypes.Contains(file.ContentType);

    private static string ToPublicUrl(SavedImageResult result, string variant)
    {
        var entry = result.RelativePaths.FirstOrDefault(x => string.Equals(x.Key.ToString(), variant, StringComparison.OrdinalIgnoreCase));
        if (string.IsNullOrWhiteSpace(entry.Value)) throw new InvalidOperationException("Image service did not return the expected variant path.");
        return "/uploads/" + entry.Value.Replace('\\', '/').TrimStart('/');
    }

    private static string? GetStoredImageId(string? imageUrl)
    {
        if (string.IsNullOrWhiteSpace(imageUrl)) return null;
        var normalized = imageUrl.Replace('\\', '/');
        if (!normalized.StartsWith("/uploads/", StringComparison.OrdinalIgnoreCase)) return null;
        var file = Path.GetFileNameWithoutExtension(normalized);
        return !string.IsNullOrWhiteSpace(file) && file.All(c => char.IsAsciiLetterOrDigit(c) || c is '-' or '_') ? file : null;
    }

    private async Task TryDeleteAsync(string imageId, CancellationToken ct)
    {
        try { await _images.DeleteAsync(imageId, UploadsRootPath, ct); }
        catch (Exception ex) { _logger.LogWarning(ex, "Could not clean up product image {ImageId}", imageId); }
    }

    private static ProductImageDto ToDto(ProductImage x)
        => new(x.Id, x.ProductId, x.ImageUrl, x.ThumbnailUrl, x.AltText, x.SortOrder, x.IsPrimary);
}
