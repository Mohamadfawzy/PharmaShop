using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Models.Dtos.Order.order;


public sealed class OrderStatusUpdateDto
{
    public byte Status { get; set; } // 1=Pending,2=Confirmed,3=Preparing,4=OutForDelivery,5=Delivered,6=Cancelled,7=Returned,8=PendingApproval
    public string? Notes { get; set; }
}
