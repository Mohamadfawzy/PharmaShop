using Contracts;
using Contracts.Images.Abstractions;
using Contracts.Images.Dtos;
using Contracts.IServices;
using Entities.Models;
using Shared.Models.Dtos.Prescription;
using Shared.Models.ImageDtos;
using Shared.Responses;

namespace Service;

public class PrescriptionService: IPrescriptionService
{
    private readonly IUnitOfWork unitOfWork;
    private readonly IImageService _imageService;

    private const string RootPath = "prescriptions";

    public PrescriptionService(IUnitOfWork unitOfWork, IImageService imageService)
    {
        this.unitOfWork = unitOfWork;
        _imageService = imageService;
    }
    public async Task<AppResponse<PrescriptionCreateResultDto>> CreateWithImagesAsync(
    PrescriptionCreateRequestDto dto,
    IReadOnlyList<UploadStreamFile> files,
    CancellationToken ct)
    {
        // 1) Validate request
        if (dto is null)
            return AppResponse<PrescriptionCreateResultDto>.ValidationError("Request is required");

        if (dto.CustomerId <= 0)
            return AppResponse<PrescriptionCreateResultDto>.ValidationErrors(
                new Dictionary<string, string[]> { ["CustomerId"] = new[] { "CustomerId is required" } },
                detail: "Validation failed"
            );

        if (dto.StoreId <= 0)
            return AppResponse<PrescriptionCreateResultDto>.ValidationErrors(
                new Dictionary<string, string[]> { ["StoreId"] = new[] { "StoreId is required" } },
                detail: "Validation failed"
            );

        if (files is null || files.Count == 0)
            return AppResponse<PrescriptionCreateResultDto>.ValidationErrors(
                new Dictionary<string, string[]> { ["Files"] = new[] { "At least one image is required" } },
                detail: "Validation failed"
            );

        // 2) Create prescription entity (Submitted)
        var now = DateTime.UtcNow;

        var prescription = new Prescription
        {
            CustomerId = dto.CustomerId,
            StoreId = dto.StoreId,

            Status = 1, // 1=Submitted
            StatusUpdatedAt = now,

            ReviewedBy = null,
            ReadyForCheckoutAt = null,

            RejectReason = null,
            Notes = string.IsNullOrWhiteSpace(dto.Notes) ? null : dto.Notes.Trim(),

            CreatedAt = now,
            UpdatedAt = null
        };

        // 3) Save prescription to get Id
        await unitOfWork.Prescriptions.AddPrescriptionAsync(prescription, ct);
        await unitOfWork.CompleteAsync(ct);

        // 4) Save images one by one using SaveAsync
        var savedResults = new List<SavedImageResult>(files.Count);

        try
        {
            var prefix = $"rx-{prescription.Id}";

            for (var i = 0; i < files.Count; i++)
            {
                ct.ThrowIfCancellationRequested();

                // IMPORTANT: Ensure stream position is at 0 if seekable
                var stream = files[i].Content;
                if (stream.CanSeek) stream.Position = 0;

                // 5) Save a single image
                var saved = await _imageService.SaveAsync(
                    imageData: stream,
                    rootPath: RootPath,
                    prefix: prefix,
                    outputFormat: ImageOutputFormat.Jpeg,
                    ct: ct);

                savedResults.Add(saved);
            }

            // 6) Determine primary index
            var primaryIndex = (dto.PrimaryIndex.HasValue && dto.PrimaryIndex.Value >= 0 && dto.PrimaryIndex.Value < savedResults.Count)
                ? dto.PrimaryIndex.Value
                : 0;

            // 7) Build PrescriptionImages entities (prefer medium variant)
            var imagesEntities = new List<PrescriptionImage>(savedResults.Count);

            for (var i = 0; i < savedResults.Count; i++)
            {
                //var urls = MapToMediumUrls(savedResults[i]);

                imagesEntities.Add(new PrescriptionImage
                {
                    PrescriptionId = prescription.Id,
                    ImageUrl = savedResults[i].Id,
                    //ThumbnailUrl = urls.ThumbnailUrl,
                    AltText = null,

                    SortOrder = i + 1,
                    IsPrimary = (i == primaryIndex),

                    CreatedAt = now,
                    CreatedBy = $"customer-{dto.CustomerId}"
                });
            }

            // 8) Save image rows to DB
            await unitOfWork.Prescriptions.AddPrescriptionImagesRangeAsync(imagesEntities, ct);
            await unitOfWork.CompleteAsync(ct);

            // 9) Return response
            var result = new PrescriptionCreateResultDto
            {
                PrescriptionId = prescription.Id,
                Status = prescription.Status,
                CreatedAt = prescription.CreatedAt,

                ImagesCount = imagesEntities.Count,
                PrimaryImageId = imagesEntities.FirstOrDefault(x => x.IsPrimary)?.Id ?? 0,

                Images = imagesEntities.Select(x => new PrescriptionCreatedImageDto
                {
                    Id = x.Id,
                    ImageUrl = x.ImageUrl,
                    ThumbnailUrl = x.ThumbnailUrl,
                    IsPrimary = x.IsPrimary,
                    SortOrder = x.SortOrder
                }).ToList()
            };

            return AppResponse<PrescriptionCreateResultDto>.Created(
                result,
                location: $"/api/v1/prescriptions/{prescription.Id}",
                title: "Prescription created successfully"
            );
        }
        catch
        {
            // 10) Rollback saved files (best-effort)
            await RollbackSavedFilesAsync(savedResults, ct);

            // 11) Rollback prescription (best-effort)
            await RollbackPrescriptionAsync(prescription.Id, ct);

            throw;
        }

        // Future improvements:
        // - Validate customer/store existence before insert
        // - Enforce max images per prescription
        // - Add server-side content-type validation at controller boundary
    }

    private async Task RollbackSavedFilesAsync(IReadOnlyList<SavedImageResult> saved, CancellationToken ct)
    {
        // 1) Best-effort delete saved images
        foreach (var s in saved)
        {
            try { await _imageService.DeleteAsync(s.Id, RootPath, ct); }
            catch { /* best-effort */ }
        }
    }

    private async Task RollbackPrescriptionAsync(int prescriptionId, CancellationToken ct)
    {
        // 1) Best-effort delete prescription row
        var entity = await unitOfWork.Prescriptions.GetForDeleteAsync(prescriptionId, ct);
        if (entity is null) return;

        unitOfWork.Prescriptions.RemovePrescription(entity);
        await unitOfWork.CompleteAsync(ct);
    }

}
