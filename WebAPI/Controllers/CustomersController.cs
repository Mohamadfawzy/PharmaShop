using Contracts;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class CustomersController : ControllerBase
{
    private readonly ICustomerRepository repo;

    public CustomersController(ICustomerRepository repo)
    {
        this.repo = repo;
    } 

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        return Ok(await repo.GetAll());
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
