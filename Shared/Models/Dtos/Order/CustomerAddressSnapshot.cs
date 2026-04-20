using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Models.Dtos.Order;

public sealed class CustomerAddressSnapshot
{
    public int AddressId { get; set; }
    public string? Title { get; set; }
    public string City { get; set; } = default!;
    public string? Region { get; set; }
    public string Street { get; set; } = default!;
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public string? Phone { get; set; }
}