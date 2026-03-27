using Entities.Models;
using Mapster;
using Shared.Models.Dtos.Product;

namespace Shared.Mappings;


public sealed class ProductMappingRegister : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<ProductCreateDto, Product>()
            // -------- System-managed fields (ignore client input) --------
            .Ignore(d => d.Id)
            .Ignore(d => d.CreatedAt)
            .Ignore(d => d.UpdatedAt)
            .Ignore(d => d.DeletedAt)
            .Ignore(d => d.IsIntegrated)
            .Ignore(d => d.IntegratedAt)

            // -------- Normalize strings (Trim + null-if-whitespace) --------
            .Map(d => d.NameAr, s => s.NameAr.Trim())
            .Map(d => d.NameEn, s => ToNullIfWhiteSpace(s.NameEn))
            .Map(d => d.InternationalCode, s => ToNullIfWhiteSpace(s.InternationalCode))
            .Map(d => d.SearchKeywords, s => ToNullIfWhiteSpace(s.SearchKeywords))
            .Map(d => d.DescriptionAr, s => ToNullIfWhiteSpace(s.DescriptionAr))
            .Map(d => d.DescriptionEn, s => ToNullIfWhiteSpace(s.DescriptionEn))

            // -------- Promotion dates: keep as-is (validator handles logic) --------
            // .Map(d => d.PromotionStartsAt, s => s.PromotionStartsAt)
            // .Map(d => d.PromotionEndsAt, s => s.PromotionEndsAt)

            // -------- Defaults: leave to DTO defaults / DB defaults / Service --------
            // Quantity, Prices, Flags... will map normally

            ;
    }

    private static string? ToNullIfWhiteSpace(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
