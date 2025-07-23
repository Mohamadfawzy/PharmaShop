using System;
using System.Collections.Generic;

namespace Migration.Models;

public partial class Sub1Category
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public string NameEn { get; set; } = null!;

    public string? DescriptionEn { get; set; }

    public int CategoryId { get; set; }

    public string? ImageUrl { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual Category Category { get; set; } = null!;
}
