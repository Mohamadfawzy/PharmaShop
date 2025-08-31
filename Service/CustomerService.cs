using Contracts;
using Contracts.IServices;

namespace Service;

public class CustomerService : ICustomerService
{
    private readonly IUnitOfWork unitOfWork;

    public CustomerService(IUnitOfWork unitOfWork)
    {
        this.unitOfWork = unitOfWork;
    }
    public Task<object> ReadAllCustomers()
    {
        var result = unitOfWork.Customers.GetAllCustomers();
        return result;
    }
}
