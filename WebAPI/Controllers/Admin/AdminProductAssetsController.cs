using Contracts;
using Contracts.IServices;
using Entities.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shared.Models.Dtos.Product;

namespace WebAPI.Controllers.Admin;

[Route("api/v1/admin/products/{productId:int}/images")]
public sealed class AdminProductAssetsController : AdminBaseApiController
{
    private readonly IUnitOfWork _uow;
    private readonly IMyImageService _images;
    public AdminProductAssetsController(IUnitOfWork uow, IMyImageService images, IWebHostEnvironment env) : base(env) { _uow = uow; _images = images; }

    [HttpGet]
    public async Task<IActionResult> List(int productId, CancellationToken ct)
    {
        if (!await _uow.Products.AnyAsync(p => p.Id == productId && p.DeletedAt == null, ct)) return NotFound();
        var images = await _uow.ProductImages.GetAllAsync(criteria: x => x.ProductId == productId && x.DeletedAt == null, orderBy: q => q.OrderBy(x => x.SortOrder), ct: ct);
        return Ok(images.Select(x => new ProductImageDto(x.Id,x.ProductId,x.ImageUrl,x.ThumbnailUrl,x.AltText,x.SortOrder,x.IsPrimary)));
    }

    [HttpPost]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> Add(int productId, [FromForm] IFormFile image, [FromForm] string? altText, [FromForm] bool isPrimary = false, CancellationToken ct = default)
    {
        if (image is null || image.Length == 0 || image.Length > 10 * 1024 * 1024) return BadRequest("Image is required and must be <= 10MB");
        var product = await _uow.Products.GetByIdNoTrackingAsync(productId, ct); if (product is null || product.DeletedAt != null) return NotFound("Product not found");
        var store = await _uow.Stores.GetByIdNoTrackingAsync(product.StoreId, ct); if (store is null) return BadRequest("Product store not found");
        var existingCount = await _uow.ProductImages.CountAsync(x => x.ProductId == productId && x.DeletedAt == null, ct);
        isPrimary = isPrimary || existingCount == 0;
        await using var stream = image.OpenReadStream();
        var id = await _images.SaveImageAsync(stream, UploadsRootPath, "product", ct);
        if (isPrimary)
        {
            var existing = await _uow.ProductImages.GetAllAsync(criteria: x => x.ProductId == productId && x.DeletedAt == null && x.IsPrimary, asNoTracking: false, ct: ct);
            foreach (var old in existing) { old.IsPrimary = false; await _uow.ProductImages.UpdateAsync(old, ct); }
            if (existing.Count > 0) await _uow.CompleteAsync(ct);
        }
        var row = new ProductImage { PharmacyId = store.PharmacyId, ProductId = productId, ImageUrl = $"/uploads/original/{id}.jpg", ThumbnailUrl = $"/uploads/small/{id}.jpg", AltText = altText, SortOrder = existingCount, IsPrimary = isPrimary, CreatedAt = DateTime.UtcNow };
        await _uow.ProductImages.AddAsync(row, ct); await _uow.CompleteAsync(ct);
        return Ok(new ProductImageDto(row.Id,row.ProductId,row.ImageUrl,row.ThumbnailUrl,row.AltText,row.SortOrder,row.IsPrimary));
    }

    [HttpPut("{imageId:long}")]
    public async Task<IActionResult> Update(int productId, long imageId, ProductImageUpdateDto dto, CancellationToken ct)
    {
        var row = await _uow.ProductImages.FirstOrDefaultAsync(x => x.Id == imageId && x.ProductId == productId && x.DeletedAt == null, asNoTracking: false, ct: ct); if (row is null) return NotFound();
        if (dto.IsPrimary == true) { var others = await _uow.ProductImages.GetAllAsync(criteria: x => x.ProductId == productId && x.Id != imageId && x.DeletedAt == null && x.IsPrimary, asNoTracking: false, ct: ct); foreach (var x in others) { x.IsPrimary = false; await _uow.ProductImages.UpdateAsync(x, ct); } if (others.Count > 0) await _uow.CompleteAsync(ct); row.IsPrimary = true; }
        if (dto.IsPrimary == false) row.IsPrimary = false;
        if (dto.SortOrder.HasValue) row.SortOrder = Math.Max(dto.SortOrder.Value, 0); if (dto.AltText is not null) row.AltText = dto.AltText;
        await _uow.ProductImages.UpdateAsync(row, ct); await _uow.CompleteAsync(ct); return Ok(new ProductImageDto(row.Id,row.ProductId,row.ImageUrl,row.ThumbnailUrl,row.AltText,row.SortOrder,row.IsPrimary));
    }

    [HttpDelete("{imageId:long}")]
    public async Task<IActionResult> Delete(int productId, long imageId, CancellationToken ct)
    {
        var row = await _uow.ProductImages.FirstOrDefaultAsync(x => x.Id == imageId && x.ProductId == productId && x.DeletedAt == null, asNoTracking: false, ct: ct); if (row is null) return NotFound();
        row.DeletedAt = DateTime.UtcNow; row.IsPrimary = false; await _uow.ProductImages.UpdateAsync(row, ct); await _uow.CompleteAsync(ct); return NoContent();
    }
}
