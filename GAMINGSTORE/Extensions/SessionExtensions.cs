using System.Security.Claims;
using System.Text.Json;
using GAMINGSTORE.Models;

namespace GAMINGSTORE.Extensions
{
    public static class SessionExtensions
    {
        private const string GuestCartKey = "Cart_Guest";
        private const string UserCartKeyPrefix = "Cart_User_";

        public static void SetObjectAsJson(this ISession session, string key, object value)
        {
            session.SetString(key, JsonSerializer.Serialize(value));
        }

        public static T? GetObjectFromJson<T>(this ISession session, string key)
        {
            var value = session.GetString(key);
            return value == null ? default : JsonSerializer.Deserialize<T>(value);
        }

        public static ShoppingCart GetShoppingCart(this ISession session, ClaimsPrincipal? user)
        {
            return session.GetObjectFromJson<ShoppingCart>(GetCartSessionKey(user)) ?? new ShoppingCart();
        }

        public static void SetShoppingCart(this ISession session, ClaimsPrincipal? user, ShoppingCart cart)
        {
            session.SetObjectAsJson(GetCartSessionKey(user), cart);
        }

        public static void RemoveShoppingCart(this ISession session, ClaimsPrincipal? user)
        {
            session.Remove(GetCartSessionKey(user));
        }

        public static string GetCartSessionKey(ClaimsPrincipal? user)
        {
            var userId = user?.FindFirstValue(ClaimTypes.NameIdentifier);
            return string.IsNullOrEmpty(userId) ? GuestCartKey : $"{UserCartKeyPrefix}{userId}";
        }
    }
}
