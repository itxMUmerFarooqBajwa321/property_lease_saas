using System.Security.Claims;

namespace property_lease_saas.Infrastructure.Extensions
{
    public static class ClaimsPrincipalExtensions
    {
        public static string UserId(this ClaimsPrincipal user)
        {
            return user.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? throw new Exception("UserId claim not found.");
        }

        public static bool IsLandlord(this ClaimsPrincipal user)
        {
            return user.HasClaim("UserType", "Landlord");
        }

        public static bool IsTenant(this ClaimsPrincipal user)
        {
            return user.HasClaim("UserType", "Tenant");
        }

        public static bool IsMechanic(this ClaimsPrincipal user)
        {
            return user.HasClaim("UserType", "Mechanic");
        }

        public static bool IsAdmin(this ClaimsPrincipal user)
        {
            return user.HasClaim("UserType", "Admin");
        }
    }
}