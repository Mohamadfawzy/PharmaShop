using Entities.Models;
using Shared.Models.Dtos.Product;

namespace Service.Mappings;
public static class ProductMappingExtensions
{
    // From CreateDTO to Entity
    public static Product ToEntity(this ProductCreateDto dto)
    {
        if (dto == null) throw new ArgumentNullException(nameof(dto));

        return new Product
        {
            StoreId = dto.StoreId,
            CategoryId = dto.CategoryId,
            CompanyId = dto.CompanyId,

            ErpProductId = dto.ErpProductId,
            ErpStoreId = dto.ErpStoreId,
            InternationalCode = string.IsNullOrWhiteSpace(dto.InternationalCode) ? null : dto.InternationalCode.Trim(),

            NameAr = dto.NameAr.Trim(),
            NameEn = string.IsNullOrWhiteSpace(dto.NameEn) ? null : dto.NameEn.Trim(),
            DescriptionAr = dto.DescriptionAr,
            DescriptionEn = dto.DescriptionEn,
            SearchKeywords = dto.SearchKeywords,

            OuterUnitId = dto.OuterUnitId,
            InnerUnitId = dto.InnerUnitId,
            InnerPerOuter = dto.InnerPerOuter,

            OuterUnitPrice = dto.OuterUnitPrice,
            InnerUnitPrice = dto.InnerUnitPrice,

            MinOrderQty = dto.MinOrderQty,
            MaxOrderQty = dto.MaxOrderQty,
            MaxPerDayQty = dto.MaxPerDayQty,

            IsReturnable = dto.IsReturnable,
            AllowSplitSale = dto.AllowSplitSale,

            Quantity = dto.Quantity,
            HasExpiry = dto.HasExpiry,
            NearestExpiryDate = dto.NearestExpiryDate,
            LastStockSyncAt = dto.LastStockSyncAt,

            HasPromotion = dto.HasPromotion,
            PromotionDiscountPercent = dto.PromotionDiscountPercent,
            PromotionStartsAt = dto.PromotionStartsAt,
            PromotionEndsAt = dto.PromotionEndsAt,

            IsFeatured = dto.IsFeatured,

            // عادة تُدار عبر ERP Sync
            IsIntegrated = false,
            IntegratedAt = null,

            Points = dto.Points,

            RequiresPrescription = dto.RequiresPrescription,
            IsAvailable = dto.IsAvailable,
            IsActive = dto.IsActive,

            UpdatedAt = null,
            DeletedAt = null
        };
    }



    // From UpdateDto to Entity
    public static Product ToEntity(this ProductUpdateDto dto)
    {
        if (dto == null) throw new ArgumentNullException(nameof(dto));

        return new Product
        {
            NameAr = dto.Name,
        };
    }


    public static ProductSubDetailsDto ToSubDetailsDto(this Product product)
    {
        if (product == null) return null!;

        return new ProductSubDetailsDto
        {
            Id = product.Id,
        };
    }

}