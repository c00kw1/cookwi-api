using Api.Hosting.Settings;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Linq;

namespace Api.Hosting.Authorization
{
    public static class ConfigureSecurity
    {
        public static void ConfigureSso(this IServiceCollection services, IWebHostEnvironment env, SsoSettings ssoSettings)
        {
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuers = ssoSettings.Issuers
                };
                options.Authority = ssoSettings.Authority;
                options.Audience = ssoSettings.Audience;
                options.RequireHttpsMetadata = !env.IsDevelopment(); // disable https for OAuth2 provider
            });
        }

        public static void ConfigurePolicies(this IServiceCollection services, AuthenticationSettings authSettings, SsoSettings ssoSettings)
        {
            services.AddAuthorization(options =>
            {
                authSettings.Policies.ForEach(policy =>
                {
                    options.AddPolicy(policy.Name, builder =>
                    {
                        policy.Scopes.ForEach(scope => builder.Requirements.Add(new HasScopeRequirement(scope, ssoSettings.Issuers)));
                        policy.Roles.ForEach(r => builder.RequireRole(r));
                    });
                });
                options.DefaultPolicy = options.GetPolicy("default"); // this is required, so check in appsettings.json you have at least a default policy
            });
        }
    }
}
