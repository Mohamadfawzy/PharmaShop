namespace Contracts.IServices;

public interface ICustomerService
{
    Task<object> ReadAllCustomers();

}
