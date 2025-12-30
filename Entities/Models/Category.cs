namespace Entities.Models;

public partial class Category
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

    public bool IsActive { get; set; }

    public bool IsDeleted { get; set; }


    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }


    /// <summary>
    /// UTC timestamp when the image was last updated.
    /// </summary>
    public DateTime? ImageUpdatedAt{ get; set; }

    public int? ParentCategoryId { get; set; }

    // self-referencing relationship
    public virtual Category? ParentCategory { get; set; }
    public virtual ICollection<Category> SubCategories { get; set; } = new List<Category>();

    public virtual ICollection<PromoCode> PromoCodes { get; set; } = new List<PromoCode>();

    public virtual ICollection<PromotionCategory> PromotionCategories { get; set; } = new List<PromotionCategory>();
    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
}
