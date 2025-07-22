using System;
using System.Collections.Generic;

namespace Entities.Models;

public partial class PrescriptionItem
{
    public int Id { get; set; }

    public int PrescriptionId { get; set; }

    public int ProductId { get; set; }

    public int Quantity { get; set; }

    public virtual Prescription Prescription { get; set; } = null!;

    public virtual Product Product { get; set; } = null!;
}
