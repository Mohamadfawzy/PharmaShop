
namespace Contracts;

public interface ICustomerRepository
{
    Task<object> GetAll();
}
