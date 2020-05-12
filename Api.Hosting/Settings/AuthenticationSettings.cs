using System.Collections.Generic;

namespace Api.Hosting.Settings
{
    public class AuthenticationSettings
    {
        public string Domain { get; set; }
        public string Audience { get; set; }
        public string Issuer { get; set; }
        public string Tenant { get; set; }
        public Dictionary<string, string> Routes { get; set; }
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
}
