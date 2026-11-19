using Contracts.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Constants;
using Shared.Models.Dtos.Prescription;
using Shared.Models.ImageDtos;
using Shared.Responses;
using WebAPI.SpecificDtos;

namespace WebAPI.Controllers;

[Route("api/v1/prescriptions")]
[Authorize(Policy = AppPolicies.Customer)]
public sealed class PrescriptionsController : BaseApiController
{
    private readonly IPrescriptionService _service;
    private readonly IWebHostEnvironment _environment;
    public PrescriptionsController(IPrescriptionService service, IWebHostEnvironment environment)
    { _service = service; _environment = environment; }

    [HttpPost]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(26 * 1024 * 1024)]
    public async Task<IActionResult> Create([FromForm] PrescriptionCreateFormData form, CancellationToken ct)
    {
        if (form.Files is null || form.Files.Count is < 1 or > 5)
            return FromAppResponse(AppResponse<object>.ValidationError("Upload between 1 and 5 images"));
        var uploads = new List<UploadStreamFile>(form.Files.Count);
        try
        {
            foreach (var file in form.Files)
            {
                if (file.Length <= 0 || file.Length > 10 * 1024 * 1024 ||
                    !new[] { "image/jpeg", "image/png", "image/webp" }.Contains(file.ContentType, StringComparer.OrdinalIgnoreCase))
                    return FromAppResponse(AppResponse<object>.ValidationError("Each file must be JPEG, PNG or WebP and no larger than 10 MB"));
                uploads.Add(new UploadStreamFile { Content = file.OpenReadStream(), FileName = file.FileName,
                    ContentType = file.ContentType, Length = file.Length });
            }
            if (uploads.Sum(f => f.Length) > 25L * 1024 * 1024)
                return FromAppResponse(AppResponse<object>.ValidationError("Total upload size cannot exceed 25 MB"));
            try
            {
                var result = await _service.CreateWithImagesAsync(new PrescriptionCreateRequestDto
                { StoreId = form.StoreId, Notes = form.Notes, PrimaryIndex = form.PrimaryIndex }, uploads, ct);
                return FromAppResponse(result);
            }
            catch (ArgumentException ex)
            {
                return FromAppResponse(AppResponse<object>.ValidationError(ex.Message));
            }
        }
        finally { foreach (var upload in uploads) upload.Content.Dispose(); }
    }

    [HttpGet]
    public async Task<IActionResult> GetMine([FromQuery] CustomerPrescriptionListQueryDto query, CancellationToken ct)
        => FromAppResponse(await _service.GetMyPrescriptionsAsync(query, ct));

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetMineById(int id, CancellationToken ct)
        => FromAppResponse(await _service.GetMyPrescriptionAsync(id, ct));

    [HttpGet("{id:int}/images/{imageIdentifier}")]
    public async Task<IActionResult> GetMyImage(int id, string imageIdentifier, CancellationToken ct)
    {
        var access = await _service.GetPrescriptionImageIdentifierAsync(id, imageIdentifier, employee: false, ct);
        if (!access.IsSuccess) return FromAppResponse(access);
        var path = Path.Combine(Directory.GetCurrentDirectory(), "prescriptions", "medium", access.Data + ".jpg");
        return System.IO.File.Exists(path) ? PhysicalFile(path, "image/jpeg") : NotFound();
    }
}
