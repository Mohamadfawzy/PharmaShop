using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Models.Carts;

internal sealed class PointsCalcResult
{
    public int MaxRedeemablePoints { get; set; }
    public int RequestedRedeemPoints { get; set; }
    public int AppliedRedeemPoints { get; set; }
    public decimal RedeemValueMoney { get; set; }
}