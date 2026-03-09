using Contracts;
using Contracts.IServices;
using Entities.Models;
using FluentValidation;
using Mapster;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Service.Validators;
using Shared.Enums;
using Shared.Models.Dtos.Product;
using Shared.Models.Dtos.Product.Units;
using Shared.Models.RequestFeatures;
using Shared.Responses;
using System.Globalization;
namespace Service;

public class ProductService : IProductService
{
    private readonly IUnitOfWork unitOfWork;
    private readonly IMyImageService imageService;
    private readonly ILogger<ProductService> logger;
    private readonly IValidator<ProductUpdateDto> _updateValidator;
    private readonly IValidator<ProductCreateDto> createValidator;
    private readonly ICurrentUserService currentUser;
    private readonly IValidator<ProductUnitCreateDto> productUnitCreateValidator;
    private readonly IValidator<ReceiveStockDto> receiveStockValidator;

    public ProductService(
        IUnitOfWork unitOfWork,
        IMyImageService imageService,
        ILogger<ProductService> logger,
        IValidator<ProductUpdateDto> updateValidator,
        IValidator<ProductCreateDto> createValidator,
        ICurrentUserService currentUser,
        IValidator<ProductUnitCreateDto> productUnitCreateValidator,
        IValidator<ReceiveStockDto> receiveStockValidator)
    {
        this.unitOfWork = unitOfWork;
        this.imageService = imageService;
        this.logger = logger;
        _updateValidator = updateValidator;
        this.createValidator = createValidator;
        this.currentUser = currentUser;
        this.productUnitCreateValidator = productUnitCreateValidator;
        this.receiveStockValidator = receiveStockValidator;
    }

    public Task<AppResponse<int>> CreateProductAsync(ProductCreateDto dto, CancellationToken ct)
    {
        throw new NotImplementedException();
    }

    public Task<AppResponse<List<ProductDetailsDto>>> GetDeletedProductsAsync(int skip, int take, CancellationToken ct)
    {
        throw new NotImplementedException();
    }

    public Task<AppResponse<ProductDetailsDto>> GetProductByIdAsync(int productId, bool includeDeleted, CancellationToken ct)
    {
        throw new NotImplementedException();
    }

    public Task<AppResponse<List<ProductListItemDto>>> GetProductsAsync(ProductListQueryDto query, CancellationToken ct)
    {
        throw new NotImplementedException();
    }

    public Task<AppResponse> SetActiveAsync(int productId, bool isActive, ProductStateChangeDto dto, CancellationToken ct)
    {
        throw new NotImplementedException();
    }

    public Task<AppResponse> SetDeletedAsync(int productId, bool isDeleted, ProductStateChangeDto dto, CancellationToken ct)
    {
        throw new NotImplementedException();
    }

    public Task<AppResponse> UpdateProductAsync(int productId, ProductUpdateDto dto, CancellationToken ct)
    {
        throw new NotImplementedException();
    }
}