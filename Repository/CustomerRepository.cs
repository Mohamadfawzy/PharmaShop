using Contracts;
using Microsoft.EntityFrameworkCore;

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
}

