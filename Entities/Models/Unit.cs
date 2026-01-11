using System;
using System.Collections.Generic;

namespace Entities.Models;

public partial class Unit
{
    public int Id { get; set; }

    public string Code { get; set; } = null!;

    public string NameAr { get; set; } = null!;

    public string NameEn { get; set; } = null!;

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public byte[] RowVersion { get; set; } = null!;

    public virtual ICollection<ProductUnit> ProductUnitBaseUnits { get; set; } = new List<ProductUnit>();

    public virtual ICollection<ProductUnit> ProductUnitUnits { get; set; } = new List<ProductUnit>();
}
