using System.ComponentModel.DataAnnotations;

namespace Shared.Models.Dtos.Category;

public class CategoryCreateDto
{
    [Required, MaxLength(3)]
    public string Name { get; set; } = null!;
    public string NameEn { get; set; } = null!;
    public string? DescriptionEn { get; set; }
    public int? ParentCategoryId { get; set; } // optional
}
