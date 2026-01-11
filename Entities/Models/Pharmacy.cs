using System;
using System.Collections.Generic;

namespace Entities.Models;

public partial class Pharmacy
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string NameEn { get; set; } = null!;

    public string? OwnerName { get; set; }

    public string? LicenseNumber { get; set; }

    public string? PhoneNumber { get; set; }

    public string? Email { get; set; }

    public string? Address { get; set; }

    public decimal? Latitude { get; set; }

    public decimal? Longitude { get; set; }

    public DateTime CreatedAt { get; set; }

    public bool IsActive { get; set; }

    public virtual ICollection<Brand> Brands { get; set; } = new List<Brand>();

    public virtual ICollection<Customer> Customers { get; set; } = new List<Customer>();

    public virtual PointSetting? PointSetting { get; set; }

    public virtual ICollection<PointsTransaction> PointsTransactions { get; set; } = new List<PointsTransaction>();

    public virtual ICollection<ProductBatch> ProductBatches { get; set; } = new List<ProductBatch>();

    public virtual ICollection<ProductImage> ProductImages { get; set; } = new List<ProductImage>();

    public virtual ICollection<ProductInventory> ProductInventories { get; set; } = new List<ProductInventory>();

    public virtual ICollection<ProductUnit> ProductUnits { get; set; } = new List<ProductUnit>();

    public virtual ICollection<Product> Products { get; set; } = new List<Product>();

    public virtual ICollection<Promotion> Promotions { get; set; } = new List<Promotion>();

    public virtual Store? Store { get; set; }

    public virtual ICollection<Tag> Tags { get; set; } = new List<Tag>();
}
