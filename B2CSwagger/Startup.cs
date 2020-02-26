using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NSwag;
using NSwag.AspNetCore;
using NSwag.Generation.Processors.Security;

namespace B2CSwagger
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
            services.AddAuthentication(opts => opts.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(opts =>
                {
                    opts.Authority = $"https://login.microsoftonline.com/tfp/b2c-tenant-name.onmicrosoft.com/b2c_1_susi_v2/v2.0/";
                    opts.Audience = Configuration["AzureAdB2C:ClientId"];
                });
            // Add security definition and scopes to document
            services.AddOpenApiDocument(document =>
            {
                document.AddSecurity("bearer", Enumerable.Empty<string>(), new OpenApiSecurityScheme
                {
                    Type = OpenApiSecuritySchemeType.OAuth2,
                    Description = "B2C authentication",
                    Flow = OpenApiOAuth2Flow.Implicit,
                    Flows = new OpenApiOAuthFlows()
                    {
                        Implicit = new OpenApiOAuthFlow()
                        {
                            Scopes = new Dictionary<string, string>
                                {
                                    { "https://b2c-tenant-name.onmicrosoft.com/your-api/user_impersonation", "Access the api as the signed-in user" },
                                    { "https://b2c-tenant-name.onmicrosoft.com/your-api/read", "Read access to the API"},
                                    { "https://b2c-tenant-name.onmicrosoft.com/your-api/mystery_scope", "Let's find out together!"}
                                },
                            AuthorizationUrl = "https://b2c-tenant-name.b2clogin.com/b2c-tenant-name.onmicrosoft.com/oauth2/v2.0/authorize?p=b2c_1_susi_v2",
                            TokenUrl = "https://b2c-tenant-name.b2clogin.com/b2c-tenant-name.onmicrosoft.com/oauth2/v2.0/token?p=b2c_1_susi_v2"
                        },
                    }
                });

                document.OperationProcessors.Add(new AspNetCoreOperationSecurityScopeProcessor("bearer"));
            });
            services.AddControllers();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseOpenApi();
            app.UseSwaggerUi3(settings =>
            {
                settings.OAuth2Client = new OAuth2ClientSettings
                {
                    ClientId = "b2c-client-id",
                    AppName = "swagger-ui-client"
                };
            });

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
