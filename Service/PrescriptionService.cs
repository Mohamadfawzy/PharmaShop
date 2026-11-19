using Contracts;
using Contracts.Images.Abstractions;
using Contracts.Images.Dtos;
using Contracts.IServices;
using Entities.Models;
using Microsoft.EntityFrameworkCore;
using Shared.Enums.Prescription;
using Shared.Models.Dtos.Prescription;
using Shared.Models.ImageDtos;
using Shared.Responses;

namespace Service;

public class PrescriptionService: IPrescriptionService
{
    private readonly IUnitOfWork unitOfWork;
    private readonly IImageService _imageService;
    private readonly ICurrentUserService _currentUser;

    private const string RootPath = "prescriptions";

    // 1=Submitted, 2=InReview, 3=ReadyForCheckout, 4=Closed, 5=Rejected


    public PrescriptionService(IUnitOfWork unitOfWork, IImageService imageService, ICurrentUserService currentUser)
    {
        this.unitOfWork = unitOfWork;
        _imageService = imageService;
        _currentUser = currentUser;
    }
    public async Task<AppResponse<PrescriptionCreateResultDto>> CreateWithImagesAsync(
    PrescriptionCreateRequestDto dto,
    IReadOnlyList<UploadStreamFile> files,
    CancellationToken ct)
    {
        // 1) Validate request
        if (dto is null)
            return AppResponse<PrescriptionCreateResultDto>.ValidationError("Request is required");

        var customer = await FindCustomerAsync(ct);
        if (customer is null) return AppResponse<PrescriptionCreateResultDto>.NotFound("Active customer profile not found");

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

        if (files.Count > 5 || files.Any(f => f.Length <= 0 || f.Length > 10 * 1024 * 1024 ||
            !new[] { "image/jpeg", "image/png", "image/webp" }.Contains(f.ContentType, StringComparer.OrdinalIgnoreCase)) ||
            files.Sum(f => f.Length) > 25L * 1024 * 1024)
            return AppResponse<PrescriptionCreateResultDto>.ValidationError("Upload 1 to 5 JPEG, PNG or WebP images, each up to 10 MB and 25 MB total");

        var store = await unitOfWork.Stores.FirstOrDefaultAsync(s => s.Id == dto.StoreId && s.IsActive && s.DeletedAt == null, ct: ct);
        if (store is null) return AppResponse<PrescriptionCreateResultDto>.ValidationError("An active store is required");
        if (dto.PrimaryIndex.HasValue && (dto.PrimaryIndex.Value < 0 || dto.PrimaryIndex.Value >= files.Count))
            return AppResponse<PrescriptionCreateResultDto>.ValidationError("PrimaryIndex must refer to an uploaded image");

        // 2) Create prescription entity (Submitted)
        var now = DateTime.UtcNow;

        var prescription = new Prescription
        {
            CustomerId = customer.Id,
            StoreId = dto.StoreId,

            Status = 1,
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
                    CreatedBy = $"customer-{customer.Id}"
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


    public async Task<AppResponse<List<CustomerPrescriptionListItemDto>>> GetMyPrescriptionsAsync(CustomerPrescriptionListQueryDto query, CancellationToken ct)
    {
        var customer = await FindCustomerAsync(ct);
        if (customer is null) return AppResponse<List<CustomerPrescriptionListItemDto>>.NotFound("Active customer profile not found");
        if (query.Status.HasValue && (query.Status < 1 || query.Status > 5))
            return AppResponse<List<CustomerPrescriptionListItemDto>>.ValidationError("Status must be between 1 and 5");
        query.Page = Math.Max(1, query.Page);
        query.PageSize = Math.Clamp(query.PageSize, 1, 100);
        var result = await unitOfWork.Prescriptions.SearchForCustomerAsync(query, customer.Id, ct);
        return AppResponse<List<CustomerPrescriptionListItemDto>>.Ok(result.Items,
            PaginationInfo.Create(query.Page, query.PageSize, result.TotalCount));
    }

    public async Task<AppResponse<CustomerPrescriptionDetailsDto>> GetMyPrescriptionAsync(int prescriptionId, CancellationToken ct)
    {
        var customer = await FindCustomerAsync(ct);
        if (customer is null) return AppResponse<CustomerPrescriptionDetailsDto>.NotFound("Active customer profile not found");
        if (prescriptionId <= 0) return AppResponse<CustomerPrescriptionDetailsDto>.ValidationError("Invalid prescription id");
        var result = await unitOfWork.Prescriptions.GetCustomerDetailsAsync(prescriptionId, customer.Id, ct);
        return result is null ? AppResponse<CustomerPrescriptionDetailsDto>.NotFound("Prescription not found")
            : AppResponse<CustomerPrescriptionDetailsDto>.Ok(result);
    }

    public async Task<AppResponse<List<AdminPrescriptionListItemDto>>> GetEmployeePrescriptionsAsync(AdminPrescriptionListQueryDto query, CancellationToken ct)
    {
        if (query is null) return AppResponse<List<AdminPrescriptionListItemDto>>.ValidationError("Query is required");
        if (query.Status.HasValue && (query.Status < 1 || query.Status > 5))
            return AppResponse<List<AdminPrescriptionListItemDto>>.ValidationError("Status must be between 1 and 5");
        if (query.From.HasValue && query.To.HasValue && query.From > query.To)
            return AppResponse<List<AdminPrescriptionListItemDto>>.ValidationError("From must be earlier than To");
        query.Page = Math.Max(1, query.Page);
        query.PageSize = Math.Clamp(query.PageSize, 1, 100);
        var employee = await FindEmployeeAsync(ct);
        var isAdmin = _currentUser.IsInRole("Admin");
        if (employee is null && !isAdmin) return AppResponse<List<AdminPrescriptionListItemDto>>.Forbidden("Active employee profile is required");
        PagedResult<AdminPrescriptionListItemDto> result;
        if (employee is not null)
        {
            if (query.StoreId.HasValue && !await unitOfWork.Stores.AnyAsync(s => s.Id == query.StoreId && s.PharmacyId == employee.PharmacyId && s.IsActive && s.DeletedAt == null, ct))
                return AppResponse<List<AdminPrescriptionListItemDto>>.ValidationError("Store is not available to this employee");
            result = await unitOfWork.Prescriptions.SearchForPharmacyAsync(query, employee.PharmacyId, ct);
        }
        else
        {
            if (!query.StoreId.HasValue || query.StoreId.Value <= 0) return AppResponse<List<AdminPrescriptionListItemDto>>.ValidationError("StoreId is required for administrators");
            result = await unitOfWork.Prescriptions.SearchAdminAsync(query, ct);
        }
        return AppResponse<List<AdminPrescriptionListItemDto>>.Ok(result.Items,
            PaginationInfo.Create(query.Page, query.PageSize, result.TotalCount));
    }

    public async Task<AppResponse<CustomerPrescriptionDetailsDto>> GetEmployeePrescriptionAsync(int prescriptionId, CancellationToken ct)
    {
        if (prescriptionId <= 0) return AppResponse<CustomerPrescriptionDetailsDto>.ValidationError("Invalid prescription id");
        var p = await unitOfWork.Prescriptions.GetForEmployeeReviewAsync(prescriptionId, ct);
        if (p is null) return AppResponse<CustomerPrescriptionDetailsDto>.NotFound("Prescription not found");
        if (!await CanAccessPrescriptionAsync(p.Store.PharmacyId, ct)) return AppResponse<CustomerPrescriptionDetailsDto>.Forbidden("Prescription is outside your pharmacy");
        var details = await unitOfWork.Prescriptions.GetCustomerDetailsAsync(prescriptionId, p.CustomerId, ct);
        return details is null ? AppResponse<CustomerPrescriptionDetailsDto>.NotFound("Prescription not found")
            : AppResponse<CustomerPrescriptionDetailsDto>.Ok(details);
    }

    public async Task<AppResponse<string>> GetPrescriptionImageIdentifierAsync(int prescriptionId, string imageIdentifier, bool employee, CancellationToken ct)
    {
        if (prescriptionId <= 0 || string.IsNullOrWhiteSpace(imageIdentifier) ||
            imageIdentifier.Any(c => !char.IsLetterOrDigit(c) && c != '-'))
            return AppResponse<string>.ValidationError("Invalid prescription image identifier");
        var p = await unitOfWork.Prescriptions.GetForEmployeeReviewAsync(prescriptionId, ct);
        if (p is null) return AppResponse<string>.NotFound("Prescription not found");
        if (employee)
        {
            if (!await CanAccessPrescriptionAsync(p.Store.PharmacyId, ct)) return AppResponse<string>.Forbidden("Prescription is outside your pharmacy");
        }
        else
        {
            var customer = await FindCustomerAsync(ct);
            if (customer is null || customer.Id != p.CustomerId) return AppResponse<string>.NotFound("Prescription image not found");
        }
        var found = await unitOfWork.Prescriptions.GetImageIdentifierAsync(prescriptionId, imageIdentifier, ct);
        return found is null ? AppResponse<string>.NotFound("Prescription image not found") : AppResponse<string>.Ok(found);
    }


    public async Task<AppResponse<List<AdminPrescriptionListItemDto>>> GetAdminPrescriptionsAsync(
            AdminPrescriptionListQueryDto query, CancellationToken ct)
    {
        // 1) Validate required inputs
        if (query is null || !query.StoreId.HasValue || query.StoreId <= 0)
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
        var prescription = await unitOfWork.Prescriptions.GetForEmployeeReviewAsync(prescriptionId, ct);
        if (prescription is null)
            return AppResponse<PrescriptionItemCreatedDto>.NotFound("Prescription not found");
        if (!await CanAccessPrescriptionAsync(prescription.Store.PharmacyId, ct)) return AppResponse<PrescriptionItemCreatedDto>.Forbidden("Prescription is outside your pharmacy");

        // 3) Enforce status rule (MVP)
        // 1=Submitted,2=InReview,3=ReadyForCheckout,4=Closed,5=Rejected
        if (prescription.Status != 2)
            return AppResponse<PrescriptionItemCreatedDto>.BusinessRuleViolation("You can add items only when prescription is InReview");

        // 4) Validate ProductId if provided (optional but safe)
        if (dto.ProductId.HasValue)
        {
            // One-line comment: Ensure referenced product exists.
            var productExists = await unitOfWork.Products.AnyAsync(p => p.Id == dto.ProductId.Value && p.StoreId == prescription.StoreId && p.IsActive && p.IsAvailable && p.DeletedAt == null, ct);
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
        var prescription = await unitOfWork.Prescriptions.GetForEmployeeReviewAsync(prescriptionId, ct);
        if (prescription is null)
            return AppResponse<PrescriptionItemsBatchCreateResultDto>.NotFound("Prescription not found");
        if (!await CanAccessPrescriptionAsync(prescription.Store.PharmacyId, ct)) return AppResponse<PrescriptionItemsBatchCreateResultDto>.Forbidden("Prescription is outside your pharmacy");

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
                criteria: p => productIds.Contains(p.Id) && p.StoreId == prescription.StoreId && p.IsActive && p.IsAvailable && p.DeletedAt == null,
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

    public async Task<AppResponse<List<PrescriptionItemListItemDto>>> GetAdminPrescriptionItemsAsync(
    int prescriptionId, CancellationToken ct)
    {
        // 1) Validate input
        if (prescriptionId <= 0)
        {
            return AppResponse<List<PrescriptionItemListItemDto>>.ValidationErrors(
                new Dictionary<string, string[]>
                {
                    ["id"] = new[] { "Invalid prescription id" }
                },
                detail: "Validation failed"
            );
        }

        // 2) Ensure prescription exists
        var exists = await unitOfWork.Prescriptions.PrescriptionExistsAsync(prescriptionId, ct);
        if (!exists)
            return AppResponse<List<PrescriptionItemListItemDto>>.NotFound("Prescription not found");
        var scoped = await unitOfWork.Prescriptions.GetForEmployeeReviewAsync(prescriptionId, ct);
        if (scoped is null || !await CanAccessPrescriptionAsync(scoped.Store.PharmacyId, ct)) return AppResponse<List<PrescriptionItemListItemDto>>.Forbidden("Prescription is outside your pharmacy");

        // 3) Load items (with product names)
        var items = await unitOfWork.Prescriptions.GetItemsByPrescriptionIdAsync(prescriptionId, ct);

        // 4) Return response
        return AppResponse<List<PrescriptionItemListItemDto>>.Ok(
            items,
            title: "Prescription items retrieved successfully"
        );

        // Future improvements:
        // - Include prescription status in the response header
        // - Add authorization checks by storeId
    }



    public async Task<AppResponse<int>> DeletePrescriptionItemAsync(
        int prescriptionId, int itemId, CancellationToken ct)
    {
        // 1) Validate input
        if (prescriptionId <= 0)
        {
            return AppResponse<int>.ValidationErrors(
                new Dictionary<string, string[]>
                {
                    ["id"] = new[] { "Invalid prescription id" }
                },
                detail: "Validation failed"
            );
        }

        if (itemId <= 0)
        {
            return AppResponse<int>.ValidationErrors(
                new Dictionary<string, string[]>
                {
                    ["itemId"] = new[] { "Invalid itemId" }
                },
                detail: "Validation failed"
            );
        }

        // 2) Ensure prescription exists and check status
        var prescription = await unitOfWork.Prescriptions.GetForEmployeeReviewAsync(prescriptionId, ct);
        if (prescription is null)
            return AppResponse<int>.NotFound("Prescription not found");
        if (!await CanAccessPrescriptionAsync(prescription.Store.PharmacyId, ct)) return AppResponse<int>.Forbidden("Prescription is outside your pharmacy");

        // 3) Enforce status rule (MVP)
        // 1=Submitted,2=InReview,3=ReadyForCheckout,4=Closed,5=Rejected
        if (prescription.Status != 2)
            return AppResponse<int>.BusinessRuleViolation("You can delete items only when prescription is InReview");

        // 4) Load item
        var item = await unitOfWork.Prescriptions.GetItemForDeleteAsync(prescriptionId, itemId, ct);
        if (item is null)
            return AppResponse<int>.NotFound("Prescription item not found");

        // 5) Delete item
        unitOfWork.Prescriptions.RemoveItem(item);

        // 6) Persist changes
        await unitOfWork.CompleteAsync(ct);

        // 7) Return success
        return AppResponse<int>.Ok(itemId, "Prescription item deleted successfully");

        // Future improvements:
        // - Allow delete in Submitted as well (if you decide)
        // - Update prescription UpdatedAt/StatusUpdatedAt when items change
        // - Add audit logging (who deleted the item)
    }


    public async Task<AppResponse<int>> UpdatePrescriptionStatusAsync(
    int prescriptionId,
    PrescriptionStatusUpdateDto dto,
    CancellationToken ct)
    {
        // 1) Validate input
        if (prescriptionId <= 0)
        {
            return AppResponse<int>.ValidationErrors(
                new Dictionary<string, string[]>
                {
                    ["id"] = new[] { "Invalid prescription id" }
                },
                detail: "Validation failed");
        }

        if (dto is null || dto.Status < 1 || dto.Status > 5)
        {
            return AppResponse<int>.ValidationErrors(
                new Dictionary<string, string[]>
                {
                    ["Status"] = new[] { "Status must be between 1 and 5" }
                },
                detail: "Validation failed");
        }

        // 2) Load prescription
        var prescription = await unitOfWork.Prescriptions.GetForEmployeeReviewAsync(prescriptionId, ct);
        if (prescription is null)
            return AppResponse<int>.NotFound("Prescription not found");
        var employee = await FindEmployeeAsync(ct);
        var isAdmin = _currentUser.IsInRole("Admin");
        if (employee is null && !isAdmin) return AppResponse<int>.Forbidden("Active employee profile is required");
        if (employee is not null && employee.PharmacyId != prescription.Store.PharmacyId) return AppResponse<int>.Forbidden("Prescription is outside your pharmacy");

        // 3) Validate state transition
        if (!IsTransitionAllowed(prescription.Status, dto.Status))
            return AppResponse<int>.BusinessRuleViolation("Invalid status transition");

        // 4) Validate ReadyForCheckout requirements
        if (dto.Status == 3)
        {
            if (employee is null)
                return AppResponse<int>.BusinessRuleViolation("An active employee profile must approve the prescription");
            var items = await unitOfWork.Prescriptions.GetItemsByPrescriptionIdAsync(prescriptionId, ct);
            if (items.Count == 0 || items.Any(i => !i.ProductId.HasValue || !i.RequestedQuantity.HasValue || i.RequestedQuantity <= 0))
                return AppResponse<int>.BusinessRuleViolation("Add mapped products and positive quantities before marking the prescription ReadyForCheckout");
            if (await unitOfWork.Prescriptions.GetImageCountAsync(prescriptionId, ct) == 0)
                return AppResponse<int>.BusinessRuleViolation("A prescription image is required before checkout");
            var productIds = items.Select(i => i.ProductId!.Value).Distinct().ToList();
            var validProducts = await unitOfWork.Products.GetAllAsync(selector: p => p.Id,
                criteria: p => productIds.Contains(p.Id) && p.StoreId == prescription.StoreId && p.IsActive && p.IsAvailable && p.DeletedAt == null,
                asNoTracking: true, ct: ct);
            if (validProducts.Count != productIds.Count)
                return AppResponse<int>.BusinessRuleViolation("All prescription products must be active and available in the prescription store");
        }

        // 5) Validate Rejected requirements
        if (dto.Status == 5)
        {
            if (string.IsNullOrWhiteSpace(dto.RejectReason))
                return AppResponse<int>.ValidationErrors(
                    new Dictionary<string, string[]>
                    {
                        ["RejectReason"] = new[] { "RejectReason is required for Rejected" }
                    },
                    detail: "Validation failed");
        }

        // 6) Apply changes
        var now = DateTime.UtcNow;

        prescription.Status = dto.Status;
        prescription.StatusUpdatedAt = now;
        prescription.UpdatedAt = now;

        // 7) Set reviewer fields
        if (employee is not null) prescription.ReviewedBy = employee.Id;

        if (dto.Status == 3)
            prescription.ReadyForCheckoutAt = now;

        // 8) Apply reject rule
        prescription.RejectReason = dto.Status == 5
            ? dto.RejectReason!.Trim()
            : null;

        // 9) Apply notes
        if (!string.IsNullOrWhiteSpace(dto.Notes))
            prescription.Notes = dto.Notes.Trim();

        // 10) Save changes
        unitOfWork.Prescriptions.UpdatePrescription(prescription);
        await unitOfWork.CompleteAsync(ct);

        // 11) Return success
        return AppResponse<int>.Ok(prescriptionId, "Prescription status updated successfully");

        // Future improvements:
        // - Add RowVersion for concurrency
        // - Log status changes (history table)
    }

    private static bool IsTransitionAllowed(byte current, byte next)
    {
        // 1) Define allowed transitions
        return (current, next) switch
        {
            (1, 2) => true, // Submitted → InReview
            (2, 3) => true, // InReview → ReadyForCheckout
            (2, 5) => true, // InReview → Rejected
            _ => false
        };
    }

    private Task<Customer?> FindCustomerAsync(CancellationToken ct)
    {
        if (!int.TryParse(_currentUser.UserId, out var userId) || userId <= 0) return Task.FromResult<Customer?>(null);
        return unitOfWork.Customers.FirstOrDefaultAsync(c => c.UserId == userId && c.IsActive, ct: ct);
    }

    private Task<Employee?> FindEmployeeAsync(CancellationToken ct)
    {
        if (!int.TryParse(_currentUser.UserId, out var userId) || userId <= 0) return Task.FromResult<Employee?>(null);
        return unitOfWork.Employees.FirstOrDefaultAsync(e => e.UserId == userId && e.IsActive && e.DeletedAt == null, ct: ct);
    }

    private async Task<bool> CanAccessPrescriptionAsync(int pharmacyId, CancellationToken ct)
    {
        if (_currentUser.IsInRole("Admin")) return true;
        var employee = await FindEmployeeAsync(ct);
        return employee is not null && employee.PharmacyId == pharmacyId;
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
