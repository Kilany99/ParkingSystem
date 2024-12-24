using System.Security.Claims;

namespace ParkingSystem.Helpers
{
    public static class UserHelperExtensions
    {
        public static int GetUserId(this ClaimsPrincipal principal)    //this keyword makes it an extension method

        {
            var claim = principal.FindFirst(ClaimTypes.NameIdentifier);
            if (claim == null)
                throw new UnauthorizedAccessException("User ID not found in token");

            return int.Parse(claim.Value);
        }

        public static string GetUserEmail(this ClaimsPrincipal principal)
        {
            var claim = principal.FindFirst(ClaimTypes.Email);
            if (claim == null)
                throw new UnauthorizedAccessException("Email not found in token");

            return claim.Value;
        }

        public static string GetUserRole(this ClaimsPrincipal principal)
        {
            var claim = principal.FindFirst(ClaimTypes.Role);
            if (claim == null)
                throw new UnauthorizedAccessException("Role not found in token");

            return claim.Value;
        }

        public static string GetUserName(this ClaimsPrincipal principal)
        {
            var claim = principal.FindFirst(ClaimTypes.Name);
            if (claim == null)
                throw new UnauthorizedAccessException("Name not found in token");

            return claim.Value;
        }
    }

}
