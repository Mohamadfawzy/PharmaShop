using Entities.Models;
using Shared.Models.Dtos.Product;

namespace Contracts;

public interface IProductRepository : IGenericRepository<Product>
{
    Task<List<ProductUpdateDto>> GetAll();
    IQueryable<ProductSubDetailsDto> GetAllQueryable();
    IQueryable<Product> Query();
}
