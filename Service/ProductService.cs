using Contracts;
using Contracts.IServices;
using Entities.Models;
using FluentValidation;
using MapsterMapper;
using Microsoft.Extensions.Logging;
using Shared.Models.Dtos.Product;
using Shared.Models.RequestFeatures;
using Shared.Responses;
namespace Service;

public class ProductService : IProductService
{
    private readonly IUnitOfWork unitOfWork;
    private readonly IMyImageService imageService;
    private readonly ILogger<ProductService> logger;
    private readonly IValidator<ProductUpdateDto> _updateValidator;
    private readonly IValidator<ProductCreateDto> createValidator;
    private readonly ICurrentUserService currentUser;
    private readonly IValidator<ReceiveStockDto> receiveStockValidator;
    private readonly IMapper mapper;

    public ProductService(
        IUnitOfWork unitOfWork,
        IMyImageService imageService,
        ILogger<ProductService> logger,
        IValidator<ProductUpdateDto> updateValidator,
        IValidator<ProductCreateDto> createValidator,
        ICurrentUserService currentUser,
        IValidator<ReceiveStockDto> receiveStockValidator,
        IMapper mapper)
    {
        this.unitOfWork = unitOfWork;
        this.imageService = imageService;
        this.logger = logger;
        _updateValidator = updateValidator;
        this.createValidator = createValidator;
        this.currentUser = currentUser;
        this.receiveStockValidator = receiveStockValidator;
        this.mapper = mapper;
    }

    public async Task<AppResponse<int>> CreateProductAsync(ProductCreateDto dto, CancellationToken ct)
    {
        // 1) Validate DTO (FluentValidation)
        var vr = await createValidator.ValidateAsync(dto, ct);
        if (!vr.IsValid)
        {
            var fieldErrors = vr.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).Distinct().ToArray());

            return AppResponse<int>.ValidationErrors(fieldErrors, detail: "Validation failed");
        }

        // 2) FK checks
        var fkErrors = new Dictionary<string, string[]>();

        //if (!await unitOfWork.Stores.ExistsByIdAsync(dto.StoreId, ct))
        //    fkErrors["StoreId"] = new[] { "Store does not exist" };

        //if (!await unitOfWork.Categories.ExistsByIdAsync(dto.CategoryId, ct))
        //    fkErrors["CategoryId"] = new[] { "Category does not exist" };

        //if (dto.CompanyId.HasValue &&
        //    !await unitOfWork.Companies.ExistsByIdAsync(dto.CompanyId.Value, ct))
        //    fkErrors["CompanyId"] = new[] { "Company does not exist" };

        //if (!await unitOfWork.Units.ExistsByIdAsync(dto.OuterUnitId, ct))
        //    fkErrors["OuterUnitId"] = new[] { "Outer unit does not exist" };

        //if (dto.InnerUnitId.HasValue &&
        //    !await unitOfWork.Units.ExistsByIdAsync(dto.InnerUnitId.Value, ct))
        //    fkErrors["InnerUnitId"] = new[] { "Inner unit does not exist" };

        if (fkErrors.Count > 0)
            return AppResponse<int>.ValidationErrors(fkErrors, detail: "Validation failed");

        // 3) Unique check 
        var code = dto.InternationalCode?.Trim();
        //if (!string.IsNullOrWhiteSpace(code))
        //{
        //    var exists = await unitOfWork.Products.ExistsByInternationalCodeAsync(dto.StoreId, code, ct);
        //    if (exists)
        //    {
        //        return AppResponse<int>.ValidationErrors(
        //            new Dictionary<string, string[]>
        //            {
        //                ["InternationalCode"] = new[] { "InternationalCode already exists in this store" }
        //            },
        //            detail: "Validation failed"
        //        );
        //    }
        //}

        // 4) Map DTO -> Entity (Mapster)
        var product = mapper.Map<Product>(dto);

        // 5) System-managed fields (always set server-side)
        var now = DateTime.UtcNow;
        product.InternationalCode = string.IsNullOrWhiteSpace(code) ? null : code; // ensure trimmed
        product.IsIntegrated = false;
        product.IntegratedAt = null;
        product.CreatedAt = now;
        product.UpdatedAt = null;
        product.DeletedAt = null;

        // (اختياري) Trim لأسماء المنتج إن لم تكن ضمن Mapster config
        product.NameAr = product.NameAr?.Trim() ?? product.NameAr;
        product.NameEn = string.IsNullOrWhiteSpace(product.NameEn) ? null : product.NameEn.Trim();

        // 6) Save
        await unitOfWork.Products.AddAsync(product, ct);
        await unitOfWork.CompleteAsync(ct);

        // 7) Response
        var location = $"/api/v1/products/{product.Id}";
        return AppResponse<int>.Created(product.Id, location, title: "Product created successfully");
    }

    public async Task<AppResponse<List<ProductListItemDto>>> SearchProductsAsync(ProductSearchQueryParams query, CancellationToken ct)
    {
        // 1) Normalize pagination (service-level safety)
        if (query.Page < 1) query.Page = 1;
        if (query.PageSize < 1) query.PageSize = 20;
        if (query.PageSize > 200) query.PageSize = 200;

        // 2) Validate minimal requirements
        if (query.StoreId <= 0)
        {
            return AppResponse<List<ProductListItemDto>>.ValidationErrors(
                new Dictionary<string, string[]>
                {
                    ["StoreId"] = new[] { "StoreId is required" }
                },
                detail: "Validation failed"
            );
        }

        // 3) Call repository
        var result = await unitOfWork.Products.SearchAsync(query, ct);

        // 4) Build pagination info
        var pagination =  PaginationInfo.Create(query.Page, query.PageSize,result.TotalCount);

        // 5) Return response
        return AppResponse<List<ProductListItemDto>>.Ok(result.Items, pagination, title: "Products retrieved successfully");
    }



}