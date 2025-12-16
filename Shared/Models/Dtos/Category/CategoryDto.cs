namespace Shared.Models.Dtos.Category;

public class CategoryDto
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string NameEn { get; set; } = null!;
    public string? DescriptionEn { get; set; }
    public string? ImageUrl { get; set; }

    public int? ParentCategoryId { get; set; } 
    public bool IsActive { get; set; }
}
