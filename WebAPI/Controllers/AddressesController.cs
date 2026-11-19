using Contracts.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Constants;
using Shared.Models.Dtos.Customer;

namespace WebAPI.Controllers;

[Route("api/v1/addresses")]
[Authorize(Policy = AppPolicies.Customer)]
public sealed class AddressesController : BaseApiController
{
    private readonly ICustomerAddressService _service;
    private readonly ICurrentUserService _currentUser;

    public AddressesController(ICustomerAddressService service, ICurrentUserService currentUser)
    {
        _service = service;
        _currentUser = currentUser;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
        => FromAppResponse(await _service.GetMyAddressesAsync(_currentUser.UserId, ct));

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
        => FromAppResponse(await _service.GetMyAddressAsync(_currentUser.UserId, id, ct));

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CustomerAddressWriteDto dto, CancellationToken ct)
    {
        var response = await _service.CreateAsync(_currentUser.UserId, dto, ct);
        return StatusCode(response.StatusCode, response);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] CustomerAddressWriteDto dto, CancellationToken ct)
        => FromAppResponse(await _service.UpdateAsync(_currentUser.UserId, id, dto, ct));

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
        => FromAppResponse(await _service.SoftDeleteAsync(_currentUser.UserId, id, ct));

    [HttpPatch("{id:int}/default")]
    public async Task<IActionResult> SetDefault(int id, CancellationToken ct)
        => FromAppResponse(await _service.SetDefaultAsync(_currentUser.UserId, id, ct));
}
