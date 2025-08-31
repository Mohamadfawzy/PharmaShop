using Contracts;
using Contracts.IServices;
using Entities.Models;
using Microsoft.AspNetCore.Mvc;
using Repository;
using Shared.Extensions;
using Shared.Models.Dtos;
using Shared.Responses;

namespace WebAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class CustomersController : ControllerBase
{
    //private readonly ICustomerRepository repo;
    //private readonly IGenericRepository<Customer> genericRepository;
    private readonly IUnitOfWork unitOfWork;
    private readonly ICustomerService customerService;


    public CustomersController(IUnitOfWork unitOfWork, ICustomerService customerService)
    {
        this.unitOfWork = unitOfWork;
        this.customerService = customerService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var user = new UserDto();

        var response = AppResponse<UserDto>.Success(user)
            .Ensure(u => u.IsActive, "User account is not active");

        if (!response.IsSuccess)
        {
            Console.WriteLine(response.Detail); // "User account is not active"
        }

        var res = new AppResponse<UserDto>();

        res.OnFailure(r => Console.WriteLine($"Error: {r.Detail}"));


        //return Ok(await repo.GetAll());
        //return Ok(await unitOfWork.Customers.GetAllCustomers());
        return Ok(await customerService.ReadAllCustomers());
    }
    
    //[HttpPost]
    //public IActionResult CreateUser([FromBody] CreateUserDto dto)
    //{
    //    if (!ModelState.IsValid)
    //    {
    //        var response = new AppResponse<object>
    //        {
    //            Succeeded = false,
    //            ErrorCode = AppErrorCode.ValidationFailed,
    //            Errors = ModelState.Values
    //                .SelectMany(v => v.Errors)
    //                .Select(e => e.ErrorMessage)
    //                .ToList()
    //        };

    //        return BadRequest(response);
    //    }

    //    // Success response
    //    var successResponse = new AppResponse<object>
    //    {
    //        Data = new { Id = 1, dto.Username }
    //    };

    //    return Ok(successResponse);
    //}
}
