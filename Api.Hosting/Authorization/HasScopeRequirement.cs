using Microsoft.AspNetCore.Authorization;
using System;

namespace Api.Hosting.Authorization
{
    public class HasScopeRequirement : IAuthorizationRequirement
    {
        public string Scope { get; }
        public string[] Issuers { get; }

        public HasScopeRequirement(string scope, string[] issuers)
        {
            Scope = scope ?? throw new ArgumentNullException(nameof(scope));
            Issuers = issuers ?? throw new ArgumentNullException(nameof(issuers));
        }
    }
}
