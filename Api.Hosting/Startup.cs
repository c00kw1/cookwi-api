using Api.Hosting.AdminAPI;
using Api.Hosting.Authorization;
using Api.Hosting.Middlewares;
using Api.Hosting.Settings;
using Api.Hosting.Utils;
using Api.Service.Mongo;
using Api.Service.Settings;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json.Converters;
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
            #region Settings gethering

            // authentication settings
            var authSection = Configuration.GetSection("Authentication");
            var authSettings = authSection.Get<AuthenticationSettings>();
            services.Configure<AuthenticationSettings>(authSection);
            // sso settings
            var ssoSection = Configuration.GetSection("Sso");
            services.Configure<SsoSettings>(ssoSection);
            var ssoSettings = ssoSection.Get<SsoSettings>();
            // s3 settings
            var s3Settings = S3Settings.Get(Configuration["S3SettingsPath"]);
            services.AddSingleton(s3Settings);
            // mongo settings and configuration
            var mongoSettings = MongoDBSettings.Get(Configuration["MongoDBSettingsPath"]);
            services.AddSingleton(mongoSettings);
            // captcha settings
            var captchaSettings = RecaptchaSettings.Get(Configuration["RecaptchaSettingsPath"]);
            services.AddSingleton(captchaSettings);

            #endregion

            #region Injections

            services.AddSingleton<RecipesService>();
            services.AddSingleton<QuantityUnitsService>();
            services.AddSingleton<UsersService>();
            services.AddSingleton<UserInvitationsService>();
            services.AddSingleton<ContactService>();
            // to handle tokens for SSO Admin API
            services.AddSingleton(typeof(TokensFactory));
            // to handle s3 operations
            services.AddSingleton(typeof(S3));

            #endregion

            services.AddControllers().AddNewtonsoftJson(options =>
            {
                options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
                options.SerializerSettings.Converters.Add(new StringEnumConverter());
            });
            services.AddCors();
            services.AddRouting(options => options.LowercaseUrls = true); // lower every controller route by default

            #region Security

            services.ConfigureSso(Environement, ssoSettings);
            services.ConfigurePolicies(authSettings, ssoSettings);
            services.AddSingleton<IAuthorizationHandler, HasScopeHandler>();

            #endregion

            #region Swagger

            // preparing URL for authentication
            var authUrl = ssoSettings.Authority + ssoSettings.Routes["Authorize"];
            var tokenUrl = ssoSettings.Authority + ssoSettings.Routes["Token"];
            var refreshUrl = ssoSettings.Authority + ssoSettings.Routes["Refresh"];
            // preparing the list of scopes for swagger
            var configScopes = authSettings.Policies.SelectMany(p => p.Scopes);
            var scopes = new Dictionary<string, string>(configScopes.Select(e => new KeyValuePair<string, string>(e, "Cookwi scope")));
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
                        configScopes.ToArray() // all the scopes of the api (not the profile/openid/email)
                    }
                });

                options.EnableAnnotations();
            });

            services.AddSwaggerGenNewtonsoftSupport();

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

            app.UseMiddleware<RequestResponseLoggingMiddleware>();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
