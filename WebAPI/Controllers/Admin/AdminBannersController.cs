using Contracts.Images.Abstractions;
using Contracts.Images.Dtos;
using Contracts.Images.Enums;
using Contracts.IServices;
using Microsoft.AspNetCore.Mvc;
using Shared.Enums;
using Shared.Models.Dtos.Banners;
using Shared.Responses;
using WebAPI.SpecificDtos;

namespace WebAPI.Controllers.Admin;

[Route("api/v1/admin/banners")]
public sealed class AdminBannersController : AdminBaseApiController
{
    private static readonly HashSet<string> AllowedContentTypes = new(StringComparer.OrdinalIgnoreCase)
        { "image/jpeg", "image/png", "image/webp" };
    private const long MaxImageBytes = 10 * 1024 * 1024;
    private readonly IBannerService _service;
    private readonly IImageService _images;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AdminBannersController> _logger;

    public AdminBannersController(IBannerService service, IImageService images, IConfiguration configuration,
        IWebHostEnvironment env, ILogger<AdminBannersController> logger) : base(env)
    {
        _service = service;
        _images = images;
        _configuration = configuration;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> Search([FromQuery] BannerListQueryDto query, CancellationToken ct)
        => FromAppResponse(await _service.SearchAdminAsync(query, PublicBaseUrl(), ct));

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
        => FromAppResponse(await _service.GetAdminByIdAsync(id, PublicBaseUrl(), ct));

    [HttpPost]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(11 * 1024 * 1024)]
    public async Task<IActionResult> Create([FromForm] BannerAdminFormDto form, CancellationToken ct)
    {
        if (!IsValidImage(form.Image)) return BadRequest("Upload a JPEG, PNG, or WebP image no larger than 10 MB.");
        SavedImageResult saved;
        await using (var stream = form.Image!.OpenReadStream())
        {
            try { saved = await _images.SaveAsync(stream, UploadsRootPath, "banner", ct: ct); }
            catch (ArgumentException ex) { return BadRequest(ex.Message); }
        }

        AppResponse<BannerDto> result;
        try
        {
            result = await _service.CreateAsync(form.ToCreateDto(), ToStoredUrl(saved, ImageVariant.Original),
                ToStoredUrl(saved, ImageVariant.Small), PublicBaseUrl(), ct);
        }
        catch
        {
            await TryDeleteAsync(saved.Id, CancellationToken.None);
            throw;
        }
        if (!result.IsSuccess) await TryDeleteAsync(saved.Id, ct);
        return FromAppResponse(result);
    }

    [HttpPut("{id:int}")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(11 * 1024 * 1024)]
    public async Task<IActionResult> Update(int id, [FromForm] BannerAdminFormDto form, CancellationToken ct)
    {
        if (form.Image is not null && !IsValidImage(form.Image))
            return BadRequest("Upload a JPEG, PNG, or WebP image no larger than 10 MB.");

        var old = await _service.GetAdminByIdAsync(id, PublicBaseUrl(), ct);
        if (!old.IsSuccess || old.Data is null) return FromAppResponse(old);
        SavedImageResult? saved = null;
        if (form.Image is not null)
        {
            await using var stream = form.Image.OpenReadStream();
            try { saved = await _images.SaveAsync(stream, UploadsRootPath, "banner", ct: ct); }
            catch (ArgumentException ex) { return BadRequest(ex.Message); }
        }

        AppResponse<BannerDto> result;
        try
        {
            result = await _service.UpdateAsync(id, form.ToUpdateDto(),
                saved is null ? null : ToStoredUrl(saved, ImageVariant.Original),
                saved is null ? null : ToStoredUrl(saved, ImageVariant.Small), PublicBaseUrl(), ct);
        }
        catch
        {
            if (saved is not null) await TryDeleteAsync(saved.Id, CancellationToken.None);
            throw;
        }
        if (!result.IsSuccess && saved is not null) await TryDeleteAsync(saved.Id, ct);
        else if (result.IsSuccess && saved is not null)
        {
            var oldId = GetStoredImageId(old.Data.ImageUrl);
            if (oldId is not null) await TryDeleteAsync(oldId, ct);
        }
        return FromAppResponse(result);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
        => FromAppResponse(await _service.SoftDeleteAsync(id, ct));

    [HttpPatch("{id:int}/active")]
    public async Task<IActionResult> SetActive(int id, [FromBody] BannerSetActiveDto dto, CancellationToken ct)
        => FromAppResponse(await _service.SetActiveAsync(id, dto.IsActive, ct));

    [HttpPatch("reorder")]
    public async Task<IActionResult> Reorder([FromBody] BannerReorderDto dto, CancellationToken ct)
        => FromAppResponse(await _service.ReorderAsync(dto, ct));

    private string PublicBaseUrl()
        => (_configuration["Banner:PublicBaseUrl"] ?? $"{Request.Scheme}://{Request.Host}{Request.PathBase}").TrimEnd('/');

    private static bool IsValidImage(IFormFile? file)
        => file is { Length: > 0 and <= MaxImageBytes } && AllowedContentTypes.Contains(file.ContentType);

    private static string ToStoredUrl(SavedImageResult result, ImageVariant variant)
    {
        var path = result.RelativePaths.FirstOrDefault(x => x.Key == variant).Value;
        if (string.IsNullOrWhiteSpace(path)) throw new InvalidOperationException($"Image service did not return the {variant} image path.");
        return "/uploads/" + path.Replace('\\', '/').TrimStart('/');
    }

    private static string? GetStoredImageId(string? imageUrl)
    {
        if (string.IsNullOrWhiteSpace(imageUrl)) return null;
        if (Uri.TryCreate(imageUrl, UriKind.Absolute, out var absolute)) imageUrl = absolute.AbsolutePath;
        var normalized = imageUrl.Replace('\\', '/');
        if (!normalized.StartsWith("/uploads/", StringComparison.OrdinalIgnoreCase)) return null;
        var id = Path.GetFileNameWithoutExtension(normalized);
        return !string.IsNullOrWhiteSpace(id) && id.All(c => char.IsAsciiLetterOrDigit(c) || c is '-' or '_') ? id : null;
    }

    private async Task TryDeleteAsync(string imageId, CancellationToken ct)
    {
        try { await _images.DeleteAsync(imageId, UploadsRootPath, ct); }
        catch (Exception ex) { _logger.LogWarning(ex, "Could not clean up banner image {ImageId}", imageId); }
    }
}
