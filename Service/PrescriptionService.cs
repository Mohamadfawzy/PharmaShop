using Contracts;
using Contracts.Images.Abstractions;
using Contracts.Images.Dtos;
using Contracts.IServices;
using Entities.Models;
using Shared.Enums.Prescription;
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

            Status = 2, // 1=Submitted
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


    public async Task<AppResponse<List<AdminPrescriptionListItemDto>>> GetAdminPrescriptionsAsync(
            AdminPrescriptionListQueryDto query, CancellationToken ct)
    {
        // 1) Validate required inputs
        if (query is null || query.StoreId <= 0)
        {
            return AppResponse<List<AdminPrescriptionListItemDto>>.ValidationErrors(
                new Dictionary<string, string[]>
                {
                    ["StoreId"] = new[] { "StoreId is required" }
                },
                detail: "Validation failed"
            );
        }

        // 2) Normalize pagination
        var page = query.Page < 1 ? 1 : query.Page;
        var pageSize = query.PageSize < 1 ? 20 : query.PageSize;
        if (pageSize > 200) pageSize = 200;

        query.Page = page;
        query.PageSize = pageSize;

        // 3) Validate status range (optional)
        if (query.Status.HasValue && (query.Status.Value < 1 || query.Status.Value > 5))
        {
            return AppResponse<List<AdminPrescriptionListItemDto>>.ValidationErrors(
                new Dictionary<string, string[]>
                {
                    ["Status"] = new[] { "Status must be between 1 and 5" }
                },
                detail: "Validation failed"
            );
        }

        // 4) Call repository
        var result = await unitOfWork.Prescriptions.SearchAdminAsync(query, ct);

        // 5) Build pagination info
        var pagination =  PaginationInfo.Create(page,pageSize,result.TotalCount);

        // 6) Return response
        return AppResponse<List<AdminPrescriptionListItemDto>>.Ok(
            result.Items, pagination, title: "Prescriptions retrieved successfully"
        );

        // Future improvements:
        // - Add role-based authorization checks (admin/pharmacist only)
        // - Add caching for common queue views
    }


    public async Task<AppResponse<PrescriptionItemCreatedDto>> AddPrescriptionItemAsync(
    int prescriptionId, PrescriptionItemCreateDto dto, CancellationToken ct)
    {
        // 1) Validate input
        if (prescriptionId <= 0)
        {
            return AppResponse<PrescriptionItemCreatedDto>.ValidationErrors(
                new Dictionary<string, string[]>
                {
                    ["id"] = new[] { "Invalid prescription id" }
                },
                detail: "Validation failed"
            );
        }

        if (dto is null)
            return AppResponse<PrescriptionItemCreatedDto>.ValidationError("Request body is required");

        if (string.IsNullOrWhiteSpace(dto.RequestedName))
        {
            return AppResponse<PrescriptionItemCreatedDto>.ValidationErrors(
                new Dictionary<string, string[]>
                {
                    ["RequestedName"] = new[] { "RequestedName is required" }
                },
                detail: "Validation failed"
            );
        }

        if (dto.RequestedQuantity.HasValue && dto.RequestedQuantity.Value <= 0)
        {
            return AppResponse<PrescriptionItemCreatedDto>.ValidationErrors(
                new Dictionary<string, string[]>
                {
                    ["RequestedQuantity"] = new[] { "RequestedQuantity must be greater than 0" }
                },
                detail: "Validation failed"
            );
        }

        // 2) Load prescription (admin)
        var prescription = await unitOfWork.Prescriptions.GetByIdForAdminAsync(prescriptionId, ct);
        if (prescription is null)
            return AppResponse<PrescriptionItemCreatedDto>.NotFound("Prescription not found");

        // 3) Enforce status rule (MVP)
        // 1=Submitted,2=InReview,3=ReadyForCheckout,4=Closed,5=Rejected
        if (prescription.Status != 2)
            return AppResponse<PrescriptionItemCreatedDto>.BusinessRuleViolation("You can add items only when prescription is InReview");

        // 4) Validate ProductId if provided (optional but safe)
        if (dto.ProductId.HasValue)
        {
            // One-line comment: Ensure referenced product exists.
            var productExists = await unitOfWork.Products.AnyAsync(p => p.Id == dto.ProductId.Value, ct);
            if (!productExists)
            {
                return AppResponse<PrescriptionItemCreatedDto>.ValidationErrors(
                    new Dictionary<string, string[]>
                    {
                        ["ProductId"] = new[] { "Product does not exist" }
                    },
                    detail: "Validation failed"
                );
            }
        }

        // 5) Create entity
        var now = DateTime.UtcNow;

        var item = new PrescriptionItem
        {
            PrescriptionId = prescriptionId,
            ProductId = dto.ProductId,
            RequestedName = dto.RequestedName.Trim(),
            RequestedQuantity = dto.RequestedQuantity,
            Notes = string.IsNullOrWhiteSpace(dto.Notes) ? null : dto.Notes.Trim(),
            CreatedAt = now,
            UpdatedAt = null
        };

        // 6) Save item
        await unitOfWork.Prescriptions.AddItemAsync(item, ct);
        await unitOfWork.CompleteAsync(ct);

        // 7) Return created DTO
        var created = new PrescriptionItemCreatedDto
        {
            Id = item.Id,
            PrescriptionId = item.PrescriptionId,
            ProductId = item.ProductId,
            RequestedName = item.RequestedName,
            RequestedQuantity = item.RequestedQuantity,
            Notes = item.Notes,
            CreatedAt = item.CreatedAt
        };

        return AppResponse<PrescriptionItemCreatedDto>.Created(
            created,
            location: $"/api/v1/admin/prescriptions/{prescriptionId}/items/{item.Id}",
            title: "Prescription item created successfully"
        );

        // Future improvements:
        // - Allow add items in Submitted and auto-switch to InReview
        // - Add EmployeeId auditing (CreatedBy) if needed
    }

    public async Task<AppResponse<PrescriptionItemsBatchCreateResultDto>> AddPrescriptionItemsBatchAsync(
        int prescriptionId, PrescriptionItemsBatchCreateDto dto, CancellationToken ct)
    {
        // 1) Validate input
        if (prescriptionId <= 0)
        {
            return AppResponse<PrescriptionItemsBatchCreateResultDto>.ValidationErrors(
                new Dictionary<string, string[]>
                {
                    ["id"] = new[] { "Invalid prescription id" }
                },
                detail: "Validation failed"
            );
        }

        if (dto is null || dto.Items is null || dto.Items.Count == 0)
        {
            return AppResponse<PrescriptionItemsBatchCreateResultDto>.ValidationErrors(
                new Dictionary<string, string[]>
                {
                    ["Items"] = new[] { "Items is required" }
                },
                detail: "Validation failed"
            );
        }

        // 2) Load prescription (admin)
        var prescription = await unitOfWork.Prescriptions.GetByIdForAdminAsync(prescriptionId, ct);
        if (prescription is null)
            return AppResponse<PrescriptionItemsBatchCreateResultDto>.NotFound("Prescription not found");

        // 3) Enforce status rule (MVP)
        // 1=Submitted,2=InReview,3=ReadyForCheckout,4=Closed,5=Rejected
        if (prescription.Status != 2)
            return AppResponse<PrescriptionItemsBatchCreateResultDto>.BusinessRuleViolation("You can add items only when prescription is InReview");

        // 4) Validate each item (simple)
        var fieldErrors = new Dictionary<string, string[]>();
        for (int i = 0; i < dto.Items.Count; i++)
        {
            var item = dto.Items[i];

            if (item is null)
            {
                fieldErrors[$"Items[{i}]"] = new[] { "Item is required" };
                continue;
            }

            if (string.IsNullOrWhiteSpace(item.RequestedName))
                fieldErrors[$"Items[{i}].RequestedName"] = new[] { "RequestedName is required" };

            if (item.RequestedQuantity.HasValue && item.RequestedQuantity.Value <= 0)
                fieldErrors[$"Items[{i}].RequestedQuantity"] = new[] { "RequestedQuantity must be greater than 0" };
        }

        if (fieldErrors.Count > 0)
            return AppResponse<PrescriptionItemsBatchCreateResultDto>.ValidationErrors(fieldErrors, detail: "Validation failed");

        // 5) Validate ProductIds in one query (optional but safe)
        var productIds = dto.Items
            .Where(x => x.ProductId.HasValue)
            .Select(x => x.ProductId!.Value)
            .Distinct()
            .ToList();

        if (productIds.Count > 0)
        {
            // One-line comment: Ensure all referenced products exist.
            var existingIds = await unitOfWork.Products.GetAllAsync(
                selector: p => p.Id,
                criteria: p => productIds.Contains(p.Id),
                asNoTracking: true,
                ct: ct);

            var missing = productIds.Except(existingIds).ToList();
            if (missing.Count > 0)
            {
                return AppResponse<PrescriptionItemsBatchCreateResultDto>.ValidationErrors(
                    new Dictionary<string, string[]>
                    {
                        ["ProductId"] = new[] { $"Some ProductIds do not exist: {string.Join(",", missing)}" }
                    },
                    detail: "Validation failed"
                );
            }
        }

        // 6) Build entities
        var now = DateTime.UtcNow;

        var entities = dto.Items.Select(x => new PrescriptionItem
        {
            PrescriptionId = prescriptionId,
            ProductId = x.ProductId,
            RequestedName = x.RequestedName.Trim(),
            RequestedQuantity = x.RequestedQuantity,
            Notes = string.IsNullOrWhiteSpace(x.Notes) ? null : x.Notes.Trim(),
            CreatedAt = now,
            UpdatedAt = null
        }).ToList();

        // 7) Save in one DB call
        await unitOfWork.Prescriptions.AddItemsRangeAsync(entities, ct);
        await unitOfWork.CompleteAsync(ct);

        // 8) Build response
        var result = new PrescriptionItemsBatchCreateResultDto
        {
            PrescriptionId = prescriptionId,
            RequestedCount = dto.Items.Count,
            InsertedCount = entities.Count,
            CreatedItemIds = entities.Select(e => e.Id).ToList()
        };

        return AppResponse<PrescriptionItemsBatchCreateResultDto>.Ok(result, "Prescription items saved successfully");

        // Future improvements:
        // - Support upsert behavior (replace set) instead of only insert
        // - Add employee auditing (CreatedByEmployeeId) if needed
        // - Add max items limit to avoid abuse
    }

    // ============================== 
    //  Privates
    // ============================== 

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
