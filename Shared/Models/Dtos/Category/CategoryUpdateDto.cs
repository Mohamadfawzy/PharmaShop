namespace Shared.Models.Dtos.Category;

public class CategoryUpdateDto
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string NameEn { get; set; } = null!;
    public string? DescriptionEn { get; set; }
    public int? ParentCategoryId { get; set; } // optional
}
