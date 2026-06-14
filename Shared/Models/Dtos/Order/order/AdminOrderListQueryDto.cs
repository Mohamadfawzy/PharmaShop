using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Models.Dtos.Order.order;

public sealed class AdminOrderListQueryDto
{
    public int StoreId { get; set; }           // Required

    public byte? Status { get; set; }          // Optional
    public byte? PaymentStatus { get; set; }   // Optional

    public int? CustomerId { get; set; }       // Optional
    public string? OrderNumber { get; set; }   // Optional search

    public DateTime? From { get; set; }
    public DateTime? To { get; set; }

    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}