namespace Entities.Models;

public partial class Product
{
    public int Id { get; set; }

    public int CategoryId { get; set; }

    public string Name { get; set; } = null!;

    public string NameEn { get; set; } = null!;

    public string? Description { get; set; }

    public string? DescriptionEn { get; set; }

    public string Barcode { get; set; } = null!;

    public string? InternationalCode { get; set; }

    public string? StockProductCode { get; set; }

    public decimal Price { get; set; }

    public decimal? OldPrice { get; set; }

    public bool IsActive { get; set; }

    public bool IsAvailable { get; set; }

    public bool IsIntegrated { get; set; }

    public bool IsGroupOffer { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime? IntegratedAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public string? CreatedBy { get; set; }

    public decimal? Points { get; set; }

    public decimal? PromoDisc { get; set; }

    public DateTime? PromoEndDate { get; set; }


    public virtual ICollection<PrescriptionItem> PrescriptionItems { get; set; } = new List<PrescriptionItem>();

    public virtual ICollection<ProductImage> ProductImages { get; set; } = new List<ProductImage>();

    public virtual ICollection<PromoCode> PromoCodes { get; set; } = new List<PromoCode>();

    public virtual ICollection<PromotionProduct> PromotionProducts { get; set; } = new List<PromotionProduct>();

    public virtual ICollection<SalesDetail> SalesDetails { get; set; } = new List<SalesDetail>();

    public virtual ICollection<SalesDetailsReturn> SalesDetailsReturns { get; set; } = new List<SalesDetailsReturn>();

    public virtual Category Category { get; set; } = null!;

    public virtual ICollection<Tag> Tags { get; set; } = new List<Tag>();
}