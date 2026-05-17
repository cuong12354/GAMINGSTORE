using System.Security.Claims;
using GAMINGSTORE.Authorization;

namespace GAMINGSTORE.Extensions
{
    public static class ClaimsPrincipalExtensions
    {
        public static bool HasPermission(this ClaimsPrincipal user, string permission)
        {
            if (user == null || string.IsNullOrWhiteSpace(permission))
            {
                return false;
            }

            if (user.IsInRole("Admin"))
            {
                return true;
            }

            return user.HasClaim(c => c.Type == PermissionConstants.ClaimType && c.Value == permission);
        }
    }
}
