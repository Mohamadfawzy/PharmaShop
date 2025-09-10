using Contracts;
using Contracts.IServices;
using Microsoft.AspNetCore.Mvc;
using Service;
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
    private readonly ImageService _imageService;
    private readonly IWebHostEnvironment env;

    public CustomersController(IUnitOfWork unitOfWork, ICustomerService customerService, ImageService imageService, IWebHostEnvironment env)
    {
        this.unitOfWork = unitOfWork;
        this.customerService = customerService;
        _imageService = imageService;
        this.env = env;
    }

    [HttpPost("upload")]
    public async Task<IActionResult> Upload([FromForm] IFormFile file, CancellationToken ct)
    {
        if (file == null || file.Length == 0)
            return BadRequest("لم يتم رفع أي صورة.");

        var rootPath = Path.Combine(env.WebRootPath ?? env.ContentRootPath, "uploads");

        try
        {
            await using var stream = file.OpenReadStream();

            // استخدم اسم الملف بدون الامتداد
            var fileNameWithoutExt = Path.GetFileNameWithoutExtension(file.FileName);

            var savedFileName = await _imageService.SaveImageAsync(stream, rootPath, ct);

            var result = new
            {
                FileName = savedFileName,
                OriginalUrl = $"/uploads/original/{savedFileName}",
                MediumUrl = $"/uploads/medium/{savedFileName}",
                SmallUrl = $"/uploads/small/{savedFileName}"
            };

            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"حدث خطأ أثناء رفع الصورة: {ex.Message}");
        }
    }


    /// <summary>
    /// Test saving multiple images with 3 sizes each.
    /// </summary>
    /// <param name="files">Uploaded image files</param>
    [HttpPost("upload-multiple")]
    public async Task<IActionResult> UploadMultipleImages(List<IFormFile> files, CancellationToken ct)
    {
        if (files == null || files.Count == 0)
            return BadRequest("No files uploaded.");

        try
        {
            var rootPath = Path.Combine(env.WebRootPath ?? env.ContentRootPath, "uploads");

            var streams = new List<Stream>();
            foreach (var file in files)
            {
                if (file.Length > 0)
                    streams.Add(file.OpenReadStream());
            }

            var savedIds = await _imageService.SaveMultipleImagesAsync(streams, rootPath, ct);

            return Ok(new
            {
                Message = "Images saved successfully",
                Count = savedIds.Count,
                ImageIds = savedIds
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, "Failed to save images. Please try again."+ ex.Message);
        }
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
