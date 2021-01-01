using System.Collections.Generic;

namespace Api.Hosting.Settings
{
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
}
