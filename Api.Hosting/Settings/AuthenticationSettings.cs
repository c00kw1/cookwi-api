using System.Collections.Generic;

namespace Api.Hosting.Settings
{
    #region Sso settings

    public class SsoSettings
    {
        public string Authority { get; set; }
        public string Audience { get; set; }
        public string Issuer { get; set; }
        public Dictionary<string, string> Routes { get; set; }
        public ApiSettings Api { get; set; }
    }

    public class ApiSettings
    {
        public string BaseUrl { get; set; }
        public string TokenUrl { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string RedirectUrl { get; set; }
    }

#endregion

    #region Authentication settings

    public class AuthenticationSettings
    {
        public string SwaggerClientId { get; set; }
        public List<Policy> Policies { get; set; }
    }

    public class Policy
    {
        public string Name { get; set; }
        public List<Scope> Scopes { get; set; }
    }

    public class Scope
    {
        public string Name { get; set; }
        public string Description { get; set; }
    }

    #endregion
}
