namespace Entities.Models;

public partial class Category
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string NameEn { get; set; } = null!;

    public string? DescriptionEn { get; set; }

    public string? ImageUrl { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public int? ParentCategoryId { get; set; }

    // self-referencing relationship
    public virtual Category? ParentCategory { get; set; }  
    public virtual ICollection<Category> SubCategories { get; set; } = new List<Category>();

    public virtual ICollection<PromoCode> PromoCodes { get; set; } = new List<PromoCode>();

    public virtual ICollection<PromotionCategory> PromotionCategories { get; set; } = new List<PromotionCategory>();
    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
}
