using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Models.Dtos.Prescription;


public sealed class PrescriptionCreatedImageDto
{
    public long Id { get; set; }
    public string ImageUrl { get; set; } = default!;
    public string? ThumbnailUrl { get; set; }
    public bool IsPrimary { get; set; }
    public int SortOrder { get; set; }
}
