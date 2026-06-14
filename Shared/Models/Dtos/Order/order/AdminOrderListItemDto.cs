using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Models.Dtos.Order.order;

public sealed class AdminOrderListItemDto
{
    public int Id { get; set; }
    public string OrderNumber { get; set; } = default!;

    public int CustomerId { get; set; }
    public int StoreId { get; set; }

    public byte Status { get; set; }
    public DateTime StatusUpdatedAt { get; set; }

    public byte PaymentStatus { get; set; }

    public decimal GrandTotal { get; set; }

    public int? PrescriptionId { get; set; }

    public DateTime CreatedAt { get; set; }
}
