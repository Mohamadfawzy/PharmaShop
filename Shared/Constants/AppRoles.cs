namespace Shared.Constants;

public static class AppRoles
{
    public const string Admin = "Admin";
    public const string Customer = "Customer";
    public const string Employee = "Employee";
    public const string DeliveryAgent = "DeliveryAgent";
}

public static class AppPolicies
{
    public const string Admin = "AdminOnly";
    public const string Customer = "CustomerOnly";
    public const string Employee = "EmployeeOnly";
    public const string DeliveryAgent = "DeliveryAgentOnly";
    public const string CanManageProducts = "CanManageProducts";
    public const string CanManagePromotions = "CanManagePromotions";
    public const string CanReviewPrescriptions = "CanReviewPrescriptions";
    public const string CanManageOrders = "CanManageOrders";
    public const string CanDeliverOrders = "CanDeliverOrders";
    public const string CanManagePoints = "CanManagePoints";
    public const string CanManageUsers = "CanManageUsers";
    public const string CanManageInventory = "CanManageInventory";
    public const string CanManageCatalog = "CanManageCatalog";
    public const string CanManageStores = "CanManageStores";
}
