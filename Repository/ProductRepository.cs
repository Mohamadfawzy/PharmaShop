using Contracts;
using Entities.Models;
using Microsoft.EntityFrameworkCore;
using Service.Mappings;

namespace Repository;

public class ProductRepository : GenericRepository<Product>, IProductRepository
{
    private readonly RepositoryContext context;

    public ProductRepository(RepositoryContext context) : base(context)
    {
        this.context = context;
    }

    public async Task<object> GetAll()
    {
        var products = await context.Products.Select(x=>x.ToSubDetailsDto())
            .ToListAsync();
        return products;
    }
}
