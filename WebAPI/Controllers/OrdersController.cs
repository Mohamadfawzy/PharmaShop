using Contracts.IServices;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WebAPI.Controllers.Admin;

namespace WebAPI.Controllers;

[Route("api/Orders")]
[ApiController]
public class OrdersController : AdminBaseApiController
{
    private readonly IOrderService orderService;

    public OrdersController(IOrderService orderService)
    {
        this.orderService = orderService;
    }


}
