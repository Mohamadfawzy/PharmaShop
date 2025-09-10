using Entities.Models;

namespace Contracts;

public interface IProductRepository : IGenericRepository<Product>
{
    Task<object> GetAll();
}
