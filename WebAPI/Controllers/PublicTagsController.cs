using Contracts.IServices;
using Microsoft.AspNetCore.Mvc;
using Shared.Models.Dtos.Tag;

namespace WebAPI.Controllers;

[Route("api/v1/tags")]
public sealed class PublicTagsController : BaseApiController
{
    private readonly ITagService _service;
    public PublicTagsController(ITagService service) => _service = service;

    [HttpGet]
    public async Task<IActionResult> List([FromQuery] TagQueryDto query, CancellationToken ct)
    {
        query.IsActive = true;
        return FromAppResponse(await _service.GetTagsAsync(query, ct));
    }
}
