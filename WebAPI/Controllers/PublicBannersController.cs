using Contracts.IServices;
using Microsoft.AspNetCore.Mvc;
using Shared.Enums;

namespace WebAPI.Controllers;

[Route("api/v1/banners")]
[ApiController]
public sealed class PublicBannersController : BaseApiController
{
    private readonly IBannerService _service;
    private readonly IConfiguration _configuration;

    public PublicBannersController(IBannerService service, IConfiguration configuration)
    {
        _service = service;
        _configuration = configuration;
    }

    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] BannerPlacement? placement, CancellationToken ct)
        => FromAppResponse(await _service.GetPublicAsync(placement, PublicBaseUrl(), ct));

    private string PublicBaseUrl()
        => (_configuration["Banner:PublicBaseUrl"] ?? $"{Request.Scheme}://{Request.Host}{Request.PathBase}").TrimEnd('/');
}
