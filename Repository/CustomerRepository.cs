using Contracts;
using Entities.Models;
using Microsoft.EntityFrameworkCore;
using Shared.Extensions;
using Shared.Models.Dtos;
using Shared.Responses;
namespace Repository;

public class CustomerRepository : GenericRepository<Customer>, ICustomerRepository
{
    private readonly RepositoryContext context;

    public CustomerRepository(RepositoryContext context) : base(context)
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


    //public async Task<AppResponse<List<string>>> GetAll2()
    //{
    //    var user = new UserDto();

    //    var response = AppResponse<UserDto>.Ok(user)
    //        .Ensure(u => u.IsActive, "User account is not active");

    //    if (!response.IsSuccess)
    //    {
    //        Console.WriteLine(response.Detail); // "User account is not active"
    //    }

    //    var list = new List<string>()
    //    {
    //        "mohamed",
    //        "ahmed",
    //        "ibrahem"
    //    };

    //    return  AppResponse<List<string>>.Ok(list, PaginationInfo.Create(1, 2, 3));
    //}

    public async Task<object> GetAllCustomers()
    {
        var customers = await context.Customers
                   .ToListAsync();
        //var result = PagedResult<CustomerDTO>.Create(customers);
        return customers;
    }
}

