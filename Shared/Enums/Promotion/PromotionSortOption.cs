using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Enums.Promotion;


public enum PromotionSortOption : byte
{
    Newest = 1,
    Oldest = 2,
    StartAtAsc = 3,
    StartAtDesc = 4
}
