using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Enums.Cart;


public enum CartStatus : byte
{
    Active = 1,
    CheckedOut = 2,
    Abandoned = 3,
    Expired = 4,
    Invalid = 5
}
