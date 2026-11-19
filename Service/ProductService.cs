using Contracts;
using Contracts.IServices;
using Entities.Models;
using FluentValidation;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;
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

        if (!await unitOfWork.Stores.AnyAsync(x => x.Id == dto.StoreId && x.DeletedAt == null && x.IsActive, ct))
            fkErrors["StoreId"] = ["Store does not exist or is inactive"];

        if (!await unitOfWork.Categories.AnyAsync(x => x.Id == dto.CategoryId && !x.IsDeleted && x.IsActive, ct))
            fkErrors["CategoryId"] = ["Category does not exist or is inactive"];

        if (dto.CompanyId.HasValue && !await unitOfWork.Companies.AnyAsync(x => x.Id == dto.CompanyId && x.DeletedAt == null && x.IsActive, ct))
            fkErrors["CompanyId"] = ["Company does not exist or is inactive"];

        if (!await unitOfWork.Units.AnyAsync(x => x.Id == dto.OuterUnitId && x.IsActive, ct))
            fkErrors["OuterUnitId"] = ["Outer unit does not exist or is inactive"];

        if (dto.InnerUnitId.HasValue && !await unitOfWork.Units.AnyAsync(x => x.Id == dto.InnerUnitId && x.IsActive, ct))
            fkErrors["InnerUnitId"] = ["Inner unit does not exist or is inactive"];

        if (fkErrors.Count > 0)
            return AppResponse<int>.ValidationErrors(fkErrors, detail: "Validation failed");

        // 3) Unique check 
        var code = dto.InternationalCode?.Trim();
        if (!string.IsNullOrWhiteSpace(code) && await unitOfWork.Products.AnyAsync(x => x.StoreId == dto.StoreId && x.InternationalCode == code && x.DeletedAt == null, ct))
            return AppResponse<int>.ValidationErrors(new Dictionary<string, string[]> { ["InternationalCode"] = ["InternationalCode already exists in this store"] });

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

    public async Task<AppResponse<ProductDetailsDto>> GetProductAsync(int id, bool includeInactive, CancellationToken ct)
    {
        var p = await unitOfWork.Products.FirstOrDefaultAsync(x => x.Id == id && x.DeletedAt == null && (includeInactive || (x.IsActive && x.Store.IsActive && x.Store.DeletedAt == null)),
            include: q => q.Include(x => x.Category).Include(x => x.Company).Include(x => x.OuterUnit).Include(x => x.InnerUnit).Include(x => x.Store).Include(x => x.ProductImages.Where(i => i.DeletedAt == null)).Include(x => x.ProductTags).ThenInclude(t => t.Tag),
            ct: ct);
        if (p is null) return AppResponse<ProductDetailsDto>.NotFound("Product not found");
        return AppResponse<ProductDetailsDto>.Ok(new ProductDetailsDto
        {
            Id = p.Id, StoreId = p.StoreId, CategoryId = p.CategoryId, CompanyId = p.CompanyId,
            NameAr = p.NameAr, NameEn = p.NameEn, DescriptionAr = p.DescriptionAr, DescriptionEn = p.DescriptionEn,
            InternationalCode = p.InternationalCode, SearchKeywords = p.SearchKeywords, OuterUnitId = p.OuterUnitId,
            InnerUnitId = p.InnerUnitId, InnerPerOuter = p.InnerPerOuter, OuterUnitPrice = p.OuterUnitPrice,
            InnerUnitPrice = p.InnerUnitPrice, Quantity = p.Quantity, IsAvailable = p.IsAvailable, IsActive = p.IsActive,
            RequiresPrescription = p.RequiresPrescription, HasPromotion = p.HasPromotion,
            PromotionDiscountPercent = p.PromotionDiscountPercent, Points = p.Points,
            PromotionStartsAt = p.PromotionStartsAt, PromotionEndsAt = p.PromotionEndsAt,
            IsFeatured = p.IsFeatured, HasExpiry = p.HasExpiry,
            Images = p.ProductImages.OrderBy(i => i.SortOrder).Select(i => i.ImageUrl).ToList(),
            TagIds = p.ProductTags.Select(t => t.TagId).ToList()
        });
    }

    public async Task<AppResponse<bool>> UpdateProductAsync(int id, ProductCreateDto dto, CancellationToken ct)
    {
        var vr = await createValidator.ValidateAsync(dto, ct);
        if (!vr.IsValid) return AppResponse<bool>.ValidationErrors(vr.Errors.GroupBy(e => e.PropertyName).ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).Distinct().ToArray()));
        var p = await unitOfWork.Products.GetByIdAsync(id, ct);
        if (p is null || p.DeletedAt != null) return AppResponse<bool>.NotFound("Product not found");
        if (!await unitOfWork.Stores.AnyAsync(x => x.Id == dto.StoreId && x.DeletedAt == null && x.IsActive, ct) || !await unitOfWork.Categories.AnyAsync(x => x.Id == dto.CategoryId && !x.IsDeleted && x.IsActive, ct) || !await unitOfWork.Units.AnyAsync(x => x.Id == dto.OuterUnitId && x.IsActive, ct) || (dto.CompanyId.HasValue && !await unitOfWork.Companies.AnyAsync(x => x.Id == dto.CompanyId && x.DeletedAt == null && x.IsActive, ct)) || (dto.InnerUnitId.HasValue && !await unitOfWork.Units.AnyAsync(x => x.Id == dto.InnerUnitId && x.IsActive, ct)))
            return AppResponse<bool>.ValidationError("Store, category or outer unit is invalid");
        var code = dto.InternationalCode?.Trim();
        if (!string.IsNullOrWhiteSpace(code) && await unitOfWork.Products.AnyAsync(x => x.Id != id && x.StoreId == dto.StoreId && x.InternationalCode == code && x.DeletedAt == null, ct))
            return AppResponse<bool>.ValidationErrors(new Dictionary<string,string[]> { ["InternationalCode"] = ["Code already exists in this store"] });
        mapper.Map(dto, p);
        p.InternationalCode = string.IsNullOrWhiteSpace(code) ? null : code;
        p.UpdatedAt = DateTime.UtcNow;
        await unitOfWork.Products.UpdateAsync(p, ct); await unitOfWork.CompleteAsync(ct);
        return AppResponse<bool>.Ok(true, "Product updated successfully");
    }

    public async Task<AppResponse<bool>> SetProductStateAsync(int id, string action, CancellationToken ct)
    {
        var p = await unitOfWork.Products.GetByIdAsync(id, ct);
        if (p is null) return AppResponse<bool>.NotFound("Product not found");
        if (action == "restore") { p.DeletedAt = null; p.IsActive = true; }
        else if (action == "delete") { p.DeletedAt = DateTime.UtcNow; p.IsActive = false; }
        else if (action == "activate") { if (p.DeletedAt != null) return AppResponse<bool>.ValidationError("Restore product first"); p.IsActive = true; }
        else if (action == "deactivate") p.IsActive = false;
        p.UpdatedAt = DateTime.UtcNow;
        await unitOfWork.Products.UpdateAsync(p, ct); await unitOfWork.CompleteAsync(ct);
        return AppResponse<bool>.Ok(true, "Product state updated");
    }

    public async Task<AppResponse<List<ProductListItemDto>>> GetDeletedProductsAsync(int page, int pageSize, CancellationToken ct)
    {
        page = Math.Max(page, 1); pageSize = Math.Clamp(pageSize, 1, 200);
        var filter = (System.Linq.Expressions.Expression<Func<Product, bool>>)(p => p.DeletedAt != null);
        var count = await unitOfWork.Products.CountAsync(filter, ct);
        var rows = await unitOfWork.Products.GetAllAsync(filter, q => q.OrderByDescending(p => p.DeletedAt), (page - 1) * pageSize, pageSize, asNoTracking: true, ct: ct);
        var data = rows.Select(p => new ProductListItemDto { Id = p.Id, NameAr = p.NameAr, NameEn = p.NameEn, OuterUnitPrice = p.OuterUnitPrice, HasPromotion = p.HasPromotion, PromotionDiscountPercent = p.PromotionDiscountPercent, Points = p.Points, RequiresPrescription = p.RequiresPrescription, IsAvailable = p.IsAvailable, IsActive = p.IsActive }).ToList();
        return AppResponse<List<ProductListItemDto>>.Ok(data, PaginationInfo.Create(page, pageSize, count));
    }



}
