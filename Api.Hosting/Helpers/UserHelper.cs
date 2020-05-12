using System.Linq;
using System.Security.Claims;

namespace Api.Hosting.Helpers
{
    public class UserHelper
    {
        public static string GetAuth0UserId(ClaimsPrincipal claimsPrincipal)
        {
            var claim = claimsPrincipal.Claims.FirstOrDefault(claim => claim.Type == ClaimTypes.NameIdentifier);
            if (claim == null || claim.Value == null || claim.Value == "")
            {
                return null;
            }

            return claim.Value;
        }
    }
}
