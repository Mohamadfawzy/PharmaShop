using System;
using System.Collections.Generic;

namespace Entities.Models;

public partial class PrescriptionImage
{
    public long Id { get; set; }

    public int PrescriptionId { get; set; }

    public string ImageUrl { get; set; } = null!;

    public string? ThumbnailUrl { get; set; }

    public string? AltText { get; set; }

    public int SortOrder { get; set; }

    public bool IsPrimary { get; set; }

    public DateTime CreatedAt { get; set; }

    public string? CreatedBy { get; set; }

    public byte[] RowVersion { get; set; } = null!;

    public virtual Prescription Prescription { get; set; } = null!;
}
