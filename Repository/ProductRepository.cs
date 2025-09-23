using Contracts;
using Entities.Models;
using Mapster;
using Microsoft.EntityFrameworkCore;
using Shared.Models.Dtos.Product;

namespace Repository;

public class ProductRepository : GenericRepository<Product>, IProductRepository
{
    private readonly RepositoryContext context;

    public ProductRepository(RepositoryContext context) : base(context)
    {
        this.context = context;
    }

    public async Task<List<ProductUpdateDto>> GetAll()
    {
        var products = await context.Products
            .ProjectToType<ProductUpdateDto>()
            .ToListAsync();
        return products;
    }

    public IQueryable<ProductSubDetailsDto> GetAllQueryable()
    {
        return context.Products.ProjectToType<ProductSubDetailsDto>();
    }

    public IQueryable<Product> Query()
    {
        return context.Products.AsQueryable();
    }

}
