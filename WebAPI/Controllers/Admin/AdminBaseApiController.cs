using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers.Admin;


[Authorize(Roles = "Admin")] // عدّلها حسب نظامك
public abstract class AdminBaseApiController : BaseApiController
{
    private readonly IWebHostEnvironment _env;

    protected AdminBaseApiController(IWebHostEnvironment env)
    {
        _env = env;
    }

    protected string UploadsRootPath
        => Path.Combine(_env.WebRootPath ?? _env.ContentRootPath, "uploads");
}