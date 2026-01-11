using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Shared.Enums;
using Shared.Responses;
using WebAPI.ApiConventions;

namespace WebAPI.Controllers;


[ApiController]
[Produces("application/json")]
[ApiConventionType(typeof(AppResponseApiConventions))]

public abstract class BaseApiController : ControllerBase
{
    protected IActionResult FromAppResponse<T>(AppResponse<T> response)
        => response.IsSuccess
            ? Ok(response)
            : StatusCode((int)response.StatusCode, response);
   
    protected IActionResult FromAppResponse(AppResponse response)
        => response.IsSuccess
            ? Ok(response)
            : StatusCode((int)response.StatusCode, response);

    // مساعد شائع عشان  لو احتجته في كنترولرز أخرى
    protected IActionResult ValidationError(string message)
        => BadRequest(AppResponse.Fail(message, AppErrorCode.ValidationError));
}