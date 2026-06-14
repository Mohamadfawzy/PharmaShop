using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Enums.Product;

public enum ProductSortOption
{
    // Name sorting
    Id = 6,
    NameAsc = 0,
    NameDesc = 1,

    // Price sorting
    PriceAsc = 2,
    PriceDesc = 3,

    // Date sorting
    Newest = 4,
    Oldest = 5,
    
}