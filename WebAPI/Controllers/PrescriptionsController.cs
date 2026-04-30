using Contracts.IServices;
using Microsoft.AspNetCore.Mvc;
using Shared.Enums.Prescription;
using Shared.Models.Dtos.Prescription;
using Shared.Models.ImageDtos;
using Shared.Responses;
using WebAPI.Controllers.Admin;
using WebAPI.SpecificDtos;

namespace WebAPI.Controllers;

[Route("api/v1/prescriptions")]
[ApiController]
public class PrescriptionsController : AdminBaseApiController
{

    private readonly IPrescriptionService _prescriptionService;

    public PrescriptionsController(IPrescriptionService prescriptionService)
    {
        _prescriptionService = prescriptionService;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromForm] PrescriptionCreateFormData form, CancellationToken ct)
    {
        // 1) Basic input guard (controller-level)
        if (form.Files is null || form.Files.Count == 0)
            return FromAppResponse(AppResponse<object>.ValidationError("At least one image is required"));

        // 2) Map form fields to contracts DTO (no ASP.NET types)
        var dto = new PrescriptionCreateRequestDto
        {
            CustomerId = form.CustomerId,
            StoreId = form.StoreId,
            Notes = form.Notes,
            PrimaryIndex = form.PrimaryIndex
        };

        // 3) Convert IFormFile to Stream wrappers (ASP.NET stays here only)
        var files = new List<UploadStreamFile>(form.Files.Count);
        try
        {
            for (var i = 0; i < form.Files.Count; i++)
            {
                var f = form.Files[i];

                files.Add(new UploadStreamFile
                {
                    Content = f.OpenReadStream(),
                    FileName = f.FileName,
                    ContentType = f.ContentType,
                    Length = f.Length
                });
            }

            // 4) Delegate to service
            var result = await _prescriptionService.CreateWithImagesAsync(dto, files, ct);

            // 5) Unified response
            return FromAppResponse(result);
        }
        finally
        {
            // 6) Dispose streams to avoid leaks
            foreach (var x in files)
                x.Content.Dispose();
        }

        // Future improvement: validate file types/sizes before calling service
    }



    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] AdminPrescriptionListQueryDto query, CancellationToken ct)
    {
        // 1) Delegate to service
        var result = await _prescriptionService.GetAdminPrescriptionsAsync(query, ct);

        // 2) Unified response
        return FromAppResponse(result);

        // Future improvement: enforce admin authorization policy
    }



    [HttpPost("{id:int}/items")]
    public async Task<IActionResult> AddItem(int id, [FromBody] PrescriptionItemCreateDto dto, CancellationToken ct)
    {
        // 1) Delegate to service
        var result = await _prescriptionService.AddPrescriptionItemAsync(id, dto, ct);

        // 2) Unified response
        return FromAppResponse(result);

        // Future improvement: enforce pharmacist/admin authorization policy
    }

    [HttpPost("{id:int}/items/batch")]
    public async Task<IActionResult> AddItemsBatch(int id, [FromBody] PrescriptionItemsBatchCreateDto dto, CancellationToken ct)
    {
        // 1) Delegate to service
        var result = await _prescriptionService.AddPrescriptionItemsBatchAsync(id, dto, ct);

        // 2) Unified response
        return FromAppResponse(result);

        // Future improvement: enforce pharmacist/admin authorization policy
    }

}
