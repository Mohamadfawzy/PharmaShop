using Shared.Models.Dtos.Product;

namespace WebAPI.SpecificDtos;

public class ProductCreateWithImageDto: ProductCreateDto
{
    public IFormFile? Image { get; set; }
    public List<IFormFile>? Images { get; set; }
}
