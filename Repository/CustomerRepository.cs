using Microsoft.EntityFrameworkCore;

namespace Repository;

public class CustomerRepository
{
    private readonly RepositoryContext context;

    public CustomerRepository(RepositoryContext context)
    {
        this.context = context;
    }

    public async Task<Object> GetAll()
    {
        var customers = await context.Customers.Select((x) => x)
            .ToListAsync();

        //var result = PagedResult<CustomerDTO>.Create(customers);
        return customers;
    }
}
