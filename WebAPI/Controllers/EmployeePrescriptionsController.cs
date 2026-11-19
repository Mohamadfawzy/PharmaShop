using Contracts.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Constants;
using Shared.Enums.Prescription;
using Shared.Models.Dtos.Prescription;
using Shared.Responses;
using System.IO;

namespace WebAPI.Controllers;

[Route("api/v1/employee/prescriptions")]
[Authorize(Policy = AppPolicies.CanReviewPrescriptions)]
public sealed class EmployeePrescriptionsController : BaseApiController
{
    private readonly IPrescriptionService _service;
    public EmployeePrescriptionsController(IPrescriptionService service) => _service = service;

    [HttpGet]
    public async Task<IActionResult> GetQueue([FromQuery] AdminPrescriptionListQueryDto query, CancellationToken ct)
        => FromAppResponse(await _service.GetEmployeePrescriptionsAsync(query, ct));

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetDetails(int id, CancellationToken ct)
        => FromAppResponse(await _service.GetEmployeePrescriptionAsync(id, ct));

    [HttpGet("{id:int}/images/{imageIdentifier}")]
    public async Task<IActionResult> GetImage(int id, string imageIdentifier, CancellationToken ct)
    {
        var access = await _service.GetPrescriptionImageIdentifierAsync(id, imageIdentifier, employee: true, ct);
        if (!access.IsSuccess) return FromAppResponse(access);
        var path = Path.Combine(Directory.GetCurrentDirectory(), "prescriptions", "medium", access.Data + ".jpg");
        return System.IO.File.Exists(path) ? PhysicalFile(path, "image/jpeg") : NotFound();
    }

    [HttpGet("{id:int}/items")]
    public async Task<IActionResult> GetItems(int id, CancellationToken ct)
        => FromAppResponse(await _service.GetAdminPrescriptionItemsAsync(id, ct));

    [HttpPost("{id:int}/items")]
    public async Task<IActionResult> AddItem(int id, [FromBody] PrescriptionItemCreateDto dto, CancellationToken ct)
        => FromAppResponse(await _service.AddPrescriptionItemAsync(id, dto, ct));

    [HttpPost("{id:int}/items/batch")]
    public async Task<IActionResult> AddItemsBatch(int id, [FromBody] PrescriptionItemsBatchCreateDto dto, CancellationToken ct)
        => FromAppResponse(await _service.AddPrescriptionItemsBatchAsync(id, dto, ct));

    [HttpDelete("{id:int}/items/{itemId:int}")]
    public async Task<IActionResult> DeleteItem(int id, int itemId, CancellationToken ct)
        => FromAppResponse(await _service.DeletePrescriptionItemAsync(id, itemId, ct));

    [HttpPatch("{id:int}/status")]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] PrescriptionStatusUpdateDto dto, CancellationToken ct)
        => FromAppResponse(await _service.UpdatePrescriptionStatusAsync(id, dto, ct));
}
