namespace Shared.Models.Dtos.Category;

public class CategoryDto
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string NameEn { get; set; } = null!;
    public string? Description { get; set; }
    public string? ImageId { get; set; }
    /// <summary>
    /// Stored image format enum value:
    /// 1 = Jpeg, 2 = Png, 3 = Webp.
    /// 
    /// NOTE: Stored as byte? to match SQL tinyint NULL.
    /// You can map it to your enum (StoredImageFormat) in application layer.
    /// </summary>
    public byte? ImageFormat { get; set; }

    /// <summary>
    /// Cache busting version. Increment this when image content changes.
    /// Used in URLs: /uploads/medium/{ImageId}.{ext}?v={ImageVersion}
    /// </summary>
    public int ImageVersion { get; set; } = 0;

    public int? ParentCategoryId { get; set; } 
    public bool IsActive { get; set; }
}
