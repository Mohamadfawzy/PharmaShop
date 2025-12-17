namespace Shared.Models.Dtos.Category;

public class CategoryTreeDto
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string NameEn { get; set; } = null!;
    public string? ImageUrl { get; set; }
    public bool IsActive { get; set; }
    public List<CategoryTreeDto> Children { get; set; } = new();
}
