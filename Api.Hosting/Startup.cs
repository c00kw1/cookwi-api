using Api.Hosting.Authorization;
using Api.Hosting.Settings;
using Api.Service;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
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
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // gather authentication settings
            var section = Configuration.GetSection("Authentication");
            var authSettings = section.Get<AuthenticationSettings>();
            services.Configure<AuthenticationSettings>(section);

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
                options.Authority = authSettings.Domain;
                options.Audience = authSettings.Audience;
            });

            services.AddAuthorization(options =>
            {
                authSettings.Policies.ForEach(policy =>
                {
                    options.AddPolicy(policy.Name, builder =>
                    {
                        policy.Scopes.ForEach(scope => builder.Requirements.Add(new HasScopeRequirement(scope.Name, authSettings.Domain + "/")));
                    });
                });
                options.DefaultPolicy = options.GetPolicy("default"); // this is required, so check in appsettings.json you have at least a default policy
            });

            services.AddSingleton<IAuthorizationHandler, HasScopeHandler>();

            #endregion

            #region Swagger

            // preparing URL for authentication
            var authUrl = authSettings.Domain + authSettings.Routes["Authorize"] + $"?audience={authSettings.Audience}";
            // preparing the list of scopes for swagger
            var configScopes = authSettings.Policies.SelectMany(p => p.Scopes);
            var scopes = new Dictionary<string, string>(configScopes.Select(e => new KeyValuePair<string, string>(e.Name, e.Description)));
            scopes.Add("openid", "OAuth2 basic scope"); // we add the classic scopes
            scopes.Add("profile", "OAuth2 basic scope");
            scopes.Add("email", "OAuth2 basic scope");

            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo { Title = Configuration.GetValue<string>("Title"), Version = "v1" });

                options.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
                {
                    Type = SecuritySchemeType.OAuth2,
                    Flows = new OpenApiOAuthFlows
                    {
                        Implicit = new OpenApiOAuthFlow
                        {
                            AuthorizationUrl = new Uri(authUrl, UriKind.Absolute),
                            Scopes = scopes
                        },
                        
                    }
                });

                options.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "oauth2" }
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
        }

        private string CraftConnectionString(string v)
        {
            throw new NotImplementedException();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                // allow angular front to call this API in dev
                app.UseCors(options => options.WithOrigins("http://localhost:4200").AllowAnyMethod().AllowAnyHeader());
            }

            app.UseHttpsRedirection();

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
                options.SwaggerEndpoint("/swagger/v1/swagger.json", $"{Configuration.GetValue<string>("Title")} V1");
                options.OAuthClientId(Configuration["Authentication:SwaggerClientId"]);
                options.OAuthScopeSeparator(" ");
            });

            #endregion

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
