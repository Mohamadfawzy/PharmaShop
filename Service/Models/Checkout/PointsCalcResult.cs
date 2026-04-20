using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Models.Checkout;

internal  sealed class PointsCalcResult
{
    public int MaxRedeemablePoints { get; set; }
    public int RedeemedPoints { get; set; }
    public decimal RedeemedAmount { get; set; }
}