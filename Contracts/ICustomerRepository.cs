using Entities.Models;
using Shared.Models.Dtos;
using Shared.Responses;

namespace Contracts;

public interface ICustomerRepository: IGenericRepository<Customer>
{
    Task<object> GetAllCustomers();
}