using Entities.Models;
using Shared.Enums;
using Shared.Models.RequestFeatures;

namespace Service.Extensions;
public static class ProductFilterExtensions
{
    public static IQueryable<Product> ApplyFilters(this IQueryable<Product> query, ProductParameters parameters)
    {
        if (parameters.CategoryId.HasValue)
            query = query.Where(p => p.CategoryId == parameters.CategoryId);

        if (parameters.CategoryId.HasValue)
            query = query.Where(p => p.CategoryId == parameters.CategoryId);

        //if (parameters.IsAvailable.HasValue)
        //    query = query.Where(p => p.IsAvailable == parameters.IsAvailable);

        if (parameters.IsActive.HasValue)
            query = query.Where(p => p.IsActive == parameters.IsActive);

        //if (parameters.MinPrice.HasValue)
        //    query = query.Where(p => p.Price >= parameters.MinPrice.Value);

        //if (parameters.MaxPrice.HasValue)
        //    query = query.Where(p => p.Price <= parameters.MaxPrice.Value);

        if (!string.IsNullOrWhiteSpace(parameters.Name))
            query = query.Where(p => p.Name.Contains(parameters.Name) || p.NameEn.Contains(parameters.Name));

        if (!string.IsNullOrWhiteSpace(parameters.Barcode))
            query = query.Where(p => p.Barcode == parameters.Barcode);

        if (parameters.CreatedAfter.HasValue)
            query = query.Where(p => p.CreatedAt >= parameters.CreatedAfter.Value);

        if (parameters.CreatedBefore.HasValue)
            query = query.Where(p => p.CreatedAt <= parameters.CreatedBefore.Value);

        return query;
    }


    public static IQueryable<Product> ApplySort(this IQueryable<Product> query, ProductParameters parameters)
    {
        return parameters.OrderBy switch
        {
            ProductOrderBy.UpdatedAt =>
                parameters.OrderDescending
                    ? query.OrderByDescending(p => p.UpdatedAt)
                    : query.OrderBy(p => p.UpdatedAt),

            //ProductOrderBy.Price =>
            //    parameters.OrderDescending
            //        ? query.OrderByDescending(p => p.Price)
            //        : query.OrderBy(p => p.Price),

            ProductOrderBy.Name =>
                parameters.OrderDescending
                    ? query.OrderByDescending(p => p.Name)
                    : query.OrderBy(p => p.Name),

            ProductOrderBy.Barcode =>
                parameters.OrderDescending
                    ? query.OrderByDescending(p => p.Barcode)
                    : query.OrderBy(p => p.Barcode),

            _ => // Default: CreatedAt
                parameters.OrderDescending
                    ? query.OrderByDescending(p => p.CreatedAt)
                    : query.OrderBy(p => p.CreatedAt),
        };
    }

    public static IQueryable<Product> ApplyPagination(this IQueryable<Product> query, ProductParameters parameters)
    {
        return query
            .Skip((parameters.PageNumber - 1) * parameters.PageSize)
            .Take(parameters.PageSize);
    }
}