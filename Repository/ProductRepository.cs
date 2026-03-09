using Contracts;
using Entities.Models;
using Mapster;
using Microsoft.EntityFrameworkCore;
using Shared.Enums.Product;
using Shared.Models.Dtos.Product;
using Shared.Models.RequestFeatures;
using Shared.Responses;

namespace Repository;

public class ProductRepository : GenericRepository<Product>, IProductRepository
{
    private readonly RepositoryContext context;

    public ProductRepository(RepositoryContext context) : base(context)
    {
        this.context = context;
    }


}




