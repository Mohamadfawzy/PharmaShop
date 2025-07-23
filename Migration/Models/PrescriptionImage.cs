using System;
using System.Collections.Generic;

namespace Migration.Models;

public partial class PrescriptionImage
{
    public int Id { get; set; }

    public int PrescriptionId { get; set; }

    public string FileUrl { get; set; } = null!;

    public int? SortOrder { get; set; }

    public DateTime UploadedAt { get; set; }

    public virtual Prescription Prescription { get; set; } = null!;
}
