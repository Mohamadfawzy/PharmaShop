using Entities.Models;
using Mapster;
using Shared.Models.Dtos.Product;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Mapping;

public static class ProductMapsterConfig
{
    public static void Register()
    {
        TypeAdapterConfig<ProductCreateDto, Product>
            .NewConfig()
            // هنضبطها في السيرفيس لأنهم Multi-tenant/Audit:
            .Ignore(dest => dest.Id)
            .Ignore(dest => dest.PharmacyId)
            .Ignore(dest => dest.CreatedAt)
            .Ignore(dest => dest.UpdatedAt)
            .Ignore(dest => dest.DeletedAt)
            .Ignore(dest => dest.DeletedBy)
            .Ignore(dest => dest.CreatedBy)
            .Ignore(dest => dest.UpdatedBy)
            .Ignore(dest => dest.RowVersion)

            // Normalized fields هنحسبهم في السيرفيس:
            .Ignore(dest => dest.NormalizedName)
            .Ignore(dest => dest.NormalizedNameEn);
    }
}