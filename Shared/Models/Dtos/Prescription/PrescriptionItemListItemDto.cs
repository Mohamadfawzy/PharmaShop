using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Models.Dtos.Prescription;

public sealed class PrescriptionItemListItemDto
{
    public int Id { get; set; }
    public int PrescriptionId { get; set; }

    public int? ProductId { get; set; }
    public string RequestedName { get; set; } = default!;
    public decimal? RequestedQuantity { get; set; }
    public string? Notes { get; set; }

    public string? ProductNameAr { get; set; }
    public string? ProductNameEn { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}