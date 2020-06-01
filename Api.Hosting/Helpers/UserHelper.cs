using System;
using System.Linq;
using System.Security.Claims;

namespace Api.Hosting.Helpers
{
    public static class UserHelper
    {
        public static Guid GetId(ClaimsPrincipal user)
        {
            var userId = user.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            return Guid.Parse(userId);
        }
    }
}
