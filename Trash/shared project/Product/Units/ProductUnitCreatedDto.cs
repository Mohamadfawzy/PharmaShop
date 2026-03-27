using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Models.Dtos.Product.Units;

public sealed class ProductUnitCreatedDto
{
    public int ProductUnitId { get; init; }
    public int ProductId { get; init; }
    public bool IsPrimary { get; init; }
}