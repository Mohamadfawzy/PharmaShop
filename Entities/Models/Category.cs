using System;
using System.Collections.Generic;

namespace Entities.Models;

public partial class Category
{
    public int Id { get; set; }

    public int? ParentCategoryId { get; set; }

    public string Name { get; set; } = null!;

    public string NameEn { get; set; } = null!;

    public string? Description { get; set; }

    public string? ImageId { get; set; }

    public byte? ImageFormat { get; set; }

    public int ImageVersion { get; set; }

    public bool IsActive { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public DateTime? ImageUpdatedAt { get; set; }

    public virtual ICollection<CategoryAuditLog> CategoryAuditLogs { get; set; } = new List<CategoryAuditLog>();

    public virtual ICollection<Category> InverseParentCategory { get; set; } = new List<Category>();

    public virtual Category? ParentCategory { get; set; }

    public virtual ICollection<Product> Products { get; set; } = new List<Product>();

    public virtual ICollection<Promotion> Promotions { get; set; } = new List<Promotion>();
}
