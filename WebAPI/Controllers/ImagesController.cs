using Contracts.Images.Abstractions;
using Contracts.Images.Dtos;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers;


[ApiController]
[Route("api/images")]
public sealed class ImagesController : ControllerBase
{
    private readonly IImageService _images;
    private readonly IWebHostEnvironment _env;

    public ImagesController(IImageService images, IWebHostEnvironment env)
    {
        _images = images;
        _env = env;
    }

    [HttpPost]
    public async Task<ActionResult<SavedImageResult>> Upload(IFormFile file, CancellationToken ct)
    {
        if (file is null || file.Length == 0)
            return BadRequest("Empty file.");

        var rootPath = Path.Combine(_env.ContentRootPath, "wwwroot", "uploads");

        await using var stream = file.OpenReadStream();
        var result = await _images.SaveAsync(stream, rootPath, prefix: "product", ct: ct);

        return Ok(result);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<SavedImageResult>> Replace(string id, IFormFile file, CancellationToken ct)
    {
        if (file is null || file.Length == 0)
            return BadRequest("Empty file.");

        var rootPath = Path.Combine(_env.ContentRootPath, "wwwroot", "uploads");

        await using var stream = file.OpenReadStream();
        var result = await _images.ReplaceAsync(stream, id, rootPath, ct);

        return Ok(result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id, CancellationToken ct)
    {
        var rootPath = Path.Combine(_env.ContentRootPath, "wwwroot", "uploads");
        await _images.DeleteAsync(id, rootPath, ct);
        return NoContent();
    }
}