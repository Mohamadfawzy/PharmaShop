using System;
using System.Collections.Generic;

namespace Entities.Models;

public partial class CustomerAddress
{
    public int Id { get; set; }

    public int CustomerId { get; set; }

    public int GovernorateAreaId { get; set; }

    public string Title { get; set; } = null!;

    public string City { get; set; } = null!;

    public string? Region { get; set; }

    public string Street { get; set; } = null!;

    public string? Landmark { get; set; }

    public double? Latitude { get; set; }

    public double? Longitude { get; set; }

    public bool IsDefault { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public DateTime? DeletedAt { get; set; }

    public virtual Customer Customer { get; set; } = null!;

    public virtual GovernorateArea GovernorateArea { get; set; } = null!;

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
}
