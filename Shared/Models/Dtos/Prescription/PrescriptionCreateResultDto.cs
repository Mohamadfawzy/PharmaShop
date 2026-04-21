using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Models.Dtos.Prescription;


public sealed class PrescriptionCreateResultDto
{
    public int PrescriptionId { get; set; }
    public byte Status { get; set; }           // 1=Submitted
    public DateTime CreatedAt { get; set; }

    public int ImagesCount { get; set; }
    public long PrimaryImageId { get; set; }

    public List<PrescriptionCreatedImageDto> Images { get; set; } = new();
}
