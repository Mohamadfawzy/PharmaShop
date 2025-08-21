using Contracts;
using Microsoft.EntityFrameworkCore;
using Shared.Responses;

namespace Repository;

public class CustomerRepository : ICustomerRepository
{
    private readonly RepositoryContext context;

    public CustomerRepository(RepositoryContext context)
    {
        this.context = context;
    }

    public async Task<object> GetAll()
    {
        var customers = await context.Customers
            
            .ToListAsync();

        //var result = PagedResult<CustomerDTO>.Create(customers);
        return customers;
    }


    public async Task<AppResponse> GetAll2()
    {
        var customers = await context.Customers

            .ToListAsync();

        var ddd = AppResponse.Success();


        return AppResponse.Success();
    }
}

