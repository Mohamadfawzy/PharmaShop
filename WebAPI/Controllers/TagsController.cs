using Contracts.IServices;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Shared.Models.Dtos.Tag;
using WebAPI.Controllers.Admin;

namespace WebAPI.Controllers;

[Route("api/v1/admin/Tags")]
[ApiController]
public class TagsController : AdminBaseApiController
{


    private readonly ITagService _tagService;

    public TagsController(ITagService tagService)
    {
        _tagService = tagService;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] TagCreateDto dto, CancellationToken ct)
    {
        return FromAppResponse(await _tagService.CreateTagAsync(dto, ct));
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] TagQueryDto query, CancellationToken ct)
    {
        return FromAppResponse(await _tagService.GetTagsAsync(query, ct));
    }

}
