using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Models.Dtos.Prescription;
public sealed class PrescriptionCreateForm
{
    public int CustomerId { get; set; }        // For now (later: from token)
    public int StoreId { get; set; }
    public string? Notes { get; set; }

    public int? PrimaryIndex { get; set; }     // Optional: 0-based primary image
    public List<IFormFile> Files { get; set; } = new();
}
