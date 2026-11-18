using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Shared.Constants;

namespace WebAPI.Controllers.Admin;


[Authorize(Policy = AppPolicies.Admin)]
public abstract class AdminBaseApiController : BaseApiController
{
    private readonly IWebHostEnvironment _env;

    protected AdminBaseApiController(IWebHostEnvironment env)
    {
        _env = env;
    }

    protected AdminBaseApiController()
    {
    }

    protected string UploadsRootPath
        => Path.Combine(_env.WebRootPath ?? _env.ContentRootPath, "uploads");
}
