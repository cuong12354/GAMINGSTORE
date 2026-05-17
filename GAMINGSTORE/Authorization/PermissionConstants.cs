namespace GAMINGSTORE.Authorization
{
    public static class PermissionConstants
    {
        public const string ClaimType = "Permission";

        public const string DashboardAccess = "Dashboard.Access";
        public const string AuditView = "Audit.View";
        public const string ProductManage = "Product.Manage";
        public const string OrderManage = "Order.Manage";
        public const string OrderView = "Order.View";
        public const string ReturnManage = "Return.Manage";
        public const string ReturnView = "Return.View";
        public const string CouponManage = "Coupon.Manage";
        public const string ReviewManage = "Review.Manage";
        public const string RoleManage = "Role.Manage";

        public static readonly string[] AllPermissions = new[]
        {
            DashboardAccess,
            AuditView,
            ProductManage,
            OrderManage,
            OrderView,
            ReturnManage,
            ReturnView,
            CouponManage,
            ReviewManage,
            RoleManage
        };
    }
}
