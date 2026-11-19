using Contracts;
using Entities.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shared.Models.Dtos.Product;

namespace WebAPI.Controllers.Admin;

[Route("api/v1/admin/products/{productId:int}/tags")]
public sealed class AdminProductTagsController : AdminBaseApiController
{
    private readonly IUnitOfWork _uow;
    public AdminProductTagsController(IUnitOfWork uow, IWebHostEnvironment env) : base(env) => _uow = uow;

    [HttpGet]
    public async Task<IActionResult> List(int productId, CancellationToken ct)
    {
        if (!await _uow.Products.AnyAsync(p => p.Id == productId && p.DeletedAt == null, ct)) return NotFound("Product not found");
        var rows = await _uow.ProductTags.GetAllAsync(criteria: x => x.ProductId == productId, include: q => q.Include(x => x.Tag), ct: ct);
        return Ok(rows.Select(x => new { x.TagId, x.Tag.NameAr, x.Tag.NameEn, x.Tag.IsActive }));
    }

    [HttpPut]
    public async Task<IActionResult> Replace(int productId, ProductTagsUpdateDto dto, CancellationToken ct)
    {
        if (!await _uow.Products.AnyAsync(p => p.Id == productId && p.DeletedAt == null, ct)) return NotFound("Product not found");
        var ids = dto.TagIds?.Where(x => x > 0).Distinct().ToList() ?? [];
        if (ids.Count > 0 && await _uow.Tags.CountAsync(t => ids.Contains(t.Id) && t.IsActive, ct) != ids.Count) return BadRequest("One or more tags are missing or inactive");
        var old = await _uow.ProductTags.GetAllAsync(criteria: x => x.ProductId == productId, asNoTracking: false, ct: ct);
        await _uow.ProductTags.DeleteRangeAsync(old, ct);
        if (ids.Count > 0) await _uow.ProductTags.AddRangeAsync(ids.Select(tagId => new ProductTag { ProductId = productId, TagId = tagId, CreatedAt = DateTime.UtcNow }), ct);
        await _uow.CompleteAsync(ct); return Ok(new { productId, tagIds = ids });
    }
}
