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
            Name = dto.Name,
            NameEn = dto.NameEn,
            Description = dto.Description,
            DescriptionEn = dto.DescriptionEn,
            Barcode = dto.Barcode,
            Price = dto.Price,
            OldPrice = dto.OldPrice,
            CategoryId = dto.CategoryId,
            IsAvailable = dto.IsAvailable,
            // It's best to set the domain/DB-related fields in the service (CreatedAt/IsActive).
            // But if you want to set them here:
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };
    }

    // From UpdateDto to Entity
    public static Product ToEntity(this ProductUpdateDto dto)
    {
        if (dto == null) throw new ArgumentNullException(nameof(dto));

        return new Product
        {
            Name = dto.Name,
            NameEn = dto.NameEn,
            Description = dto.Description,
            DescriptionEn = dto.DescriptionEn,
            //Barcode = dto.Barcode,
            //Price = dto.Price,
            //OldPrice = dto.OldPrice,
            //CategoryId = dto.CategoryId,
            //SubCategoryId = dto.SubCategoryId,
            //IsAvailable = dto.IsAvailable,
            IsActive = true
        };
    }


    public static ProductSubDetailsDto ToSubDetailsDto(this Product product)
    {
        if (product == null) return null!;

        return new ProductSubDetailsDto
        {
            Id = product.Id,
            Name = product.Name ?? string.Empty,
            NameEn = product.NameEn ?? string.Empty,
            Description = product.Description ?? string.Empty,
            DescriptionEn = product.DescriptionEn ?? string.Empty,
            Barcode = product.Barcode ?? string.Empty,
            Price = product.Price,
            IsAvailable = product.IsAvailable,
            Points = product.Points,
            PromoDisc = product.PromoDisc,
            PromoEndDate = product.PromoEndDate,
            IsGroupOffer = product.IsGroupOffer,
            ImageName = product.ProductImages.FirstOrDefault(pi => pi.IsMain)?.ImageUrl
                        ?? product.ProductImages.FirstOrDefault()?.ImageUrl
                        ?? string.Empty
        };
    }

}