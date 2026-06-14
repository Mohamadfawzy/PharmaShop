using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Models.Dtos.Prescription;

public sealed class PrescriptionStatusUpdateDto
{
    public byte Status { get; set; }              // Required (1..5)
    public int? ReviewedBy { get; set; }          // Required for ReadyForCheckout
    public string? RejectReason { get; set; }     // Required for Rejected
    public string? Notes { get; set; }
}