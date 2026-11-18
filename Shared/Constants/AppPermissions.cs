namespace Shared.Constants;

public static class AppPermissions
{
    public const string ClaimType = "permission";

    public static readonly IReadOnlyDictionary<string, string> PolicyPermissions =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            [AppPolicies.CanManageProducts] = AppPolicies.CanManageProducts,
            [AppPolicies.CanManagePromotions] = AppPolicies.CanManagePromotions,
            [AppPolicies.CanReviewPrescriptions] = AppPolicies.CanReviewPrescriptions,
            [AppPolicies.CanManageOrders] = AppPolicies.CanManageOrders,
            [AppPolicies.CanDeliverOrders] = AppPolicies.CanDeliverOrders,
            [AppPolicies.CanManagePoints] = AppPolicies.CanManagePoints,
            [AppPolicies.CanManageInventory] = AppPolicies.CanManageInventory,
            [AppPolicies.CanManageCatalog] = AppPolicies.CanManageCatalog
        };

    public static bool IsKnown(string permission)
        => PolicyPermissions.ContainsKey(permission);
}
