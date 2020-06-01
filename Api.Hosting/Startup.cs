using Api.Hosting.AdminAPI;
using Api.Hosting.Authorization;
using Api.Hosting.Settings;
using Api.Service;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Api.Hosting
{
    public class Startup
    {
        public Startup(IConfiguration configuration, IWebHostEnvironment env)
        {
            Configuration = configuration;
            Environement = env;
        }

        public IConfiguration Configuration { get; }
        public IWebHostEnvironment Environement { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // gather authentication and sso settings
            var authSection = Configuration.GetSection("Authentication");
            var authSettings = authSection.Get<AuthenticationSettings>();
            services.Configure<AuthenticationSettings>(authSection);

            var ssoSection = Configuration.GetSection("Sso");
            var ssoSettings = ssoSection.Get<SsoSettings>();
            services.Configure<SsoSettings>(ssoSection);
            services.AddSingleton(typeof(TokensFactory));

            services.AddControllers().AddNewtonsoftJson(options => options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore);
            services.AddCors();
            services.AddRouting(options => options.LowercaseUrls = true); // lower every controller route by default

            #region Security

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(options =>
            {
                options.Authority = ssoSettings.Authority;
                options.Audience = ssoSettings.Audience;
                options.ClaimsIssuer = ssoSettings.Issuer;
                options.RequireHttpsMetadata = !Environement.IsDevelopment(); // disable https for OAuth2 provider
            });

            services.AddAuthorization(options =>
            {
                authSettings.Policies.ForEach(policy =>
                {
                    options.AddPolicy(policy.Name, builder =>
                    {
                        policy.Scopes.ForEach(scope => builder.Requirements.Add(new HasScopeRequirement(scope.Name, ssoSettings.Issuer)));
                    });
                });
                options.DefaultPolicy = options.GetPolicy("default"); // this is required, so check in appsettings.json you have at least a default policy
            });

            services.AddSingleton<IAuthorizationHandler, HasScopeHandler>();

            #endregion

            #region Swagger

            // preparing URL for authentication
            var authUrl = ssoSettings.Authority + ssoSettings.Routes["Authorize"];
            var tokenUrl = ssoSettings.Authority + ssoSettings.Routes["Token"];
            var refreshUrl = ssoSettings.Authority + ssoSettings.Routes["Refresh"];
            // preparing the list of scopes for swagger
            var configScopes = authSettings.Policies.SelectMany(p => p.Scopes);
            var scopes = new Dictionary<string, string>(configScopes.Select(e => new KeyValuePair<string, string>(e.Name, e.Description)));
            // we add the classic scope
            scopes.Add("openid", "OAuth2 basic scope");
            scopes.Add("email", "OAuth2 basic scope");
            scopes.Add("profile", "OAuth2 basic scope");

            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("APIv1", new OpenApiInfo { Title = Configuration.GetValue<string>("Title"), Version = "v1" });
                options.SwaggerDoc("Adminv1", new OpenApiInfo { Title = Configuration.GetValue<string>("Title"), Version = "v1" });

                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Type = SecuritySchemeType.OAuth2,
                    Flows = new OpenApiOAuthFlows
                    {
                        AuthorizationCode = new OpenApiOAuthFlow
                        {
                            AuthorizationUrl = new Uri(authUrl, UriKind.Absolute),
                            TokenUrl = new Uri(tokenUrl, UriKind.Absolute),
                            RefreshUrl = new Uri(refreshUrl, UriKind.Absolute),
                            Scopes = scopes
                        }
                    }
                });

                options.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
                        },
                        configScopes.Select(s => s.Name).ToArray() // all the scopes of the api (not the profile/openid/email)
                    }
                });

                options.EnableAnnotations();
            });

            services.AddSwaggerGenNewtonsoftSupport();

            #endregion

            #region DBContext

            var dbSettings = DatabaseSettings.Get(Configuration["DatabaseSettingsPath"]);
            services.AddDbContext<CookwiDbContext>(options => options.UseNpgsql(dbSettings.ConnectionString));

            #endregion

            services.AddMvc(c => c.Conventions.Add(new ApiExplorerGroupPerNamespaceConvention()));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            // in PROD or homolo, we actually are behind a nginx proxy which handles SSL for us
            //app.UseHttpsRedirection();

            app.UseCors(options => options.WithOrigins(Configuration["Cors"].Split('|')).AllowAnyMethod().AllowAnyHeader());
            app.UseRouting();

            #region Security

            app.UseAuthentication();
            app.UseAuthorization();

            #endregion

            #region Swagger

            app.UseSwagger();
            app.UseSwaggerUI(options =>
            {
                options.OAuthAppName($"{Configuration.GetValue<string>("Title")} swagger");
                options.SwaggerEndpoint("/swagger/APIv1/swagger.json", $"{Configuration.GetValue<string>("Title")} V1");
                options.SwaggerEndpoint("/swagger/Adminv1/swagger.json", $"{Configuration.GetValue<string>("Title")} V1 Admin");
                options.OAuthClientId(Configuration["Authentication:SwaggerClientId"]);
                options.OAuthScopeSeparator(" ");
                options.OAuthUsePkce();
            });

            #endregion

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
