using System;
using System.Collections.Generic;
using System.Linq;
using IdentityServer4.Hosting;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;

[assembly: ApiController]

namespace RepostAspNet
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
            services.AddDbContext<DatabaseContext>(options =>
                options.UseInMemoryDatabase("RepostDatabase"));

            services.AddControllers(options =>
                options.Filters.Add(new ErrorResponseFilter()));

            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("openapi", new OpenApiInfo
                {
                    Title = "Repost",
                    Version = "0.0.1",
                    Description = "Repost API written in ASP.NET Core 3.1 Web APIs\n\n" +
                                  "[View source code on GitHub](URL_HERE)\n\n" +
                                  "Authors: pckv, EspenK, jonsondrem"
                });

                options.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
                {
                    Type = SecuritySchemeType.OAuth2,
                    Flows = new OpenApiOAuthFlows
                    {
                        Password = new OpenApiOAuthFlow
                        {
                            TokenUrl = new Uri("/api/auth/token", UriKind.Relative),
                            Scopes = new Dictionary<string, string>
                            {
                                {"users", "User permissions"}
                            }
                        }
                    }
                });
                options.OperationFilter<OpenApiAuthorizationCheckFilter>();
            });

            var builder = services.AddIdentityServer()
                .AddDeveloperSigningCredential()
                .AddInMemoryApiResources(Config.Apis)
                .AddInMemoryClients(Config.Clients)
                .AddResourceOwnerValidator<ResourceOwnerPasswordValidator>();

            builder.Services
                .Where(descriptor => descriptor.ServiceType == typeof(Endpoint))
                .Select(item => (Endpoint) item.ImplementationInstance)
                .ToList()
                .ForEach(item => item.Path = item.Path.Value.Replace("/connect", "/api/auth"));

            services.AddAuthentication("Bearer")
                .AddJwtBearer("Bearer", options =>
                {
                    options.Authority = "http://localhost:5000";
                    options.RequireHttpsMetadata = false;
                    options.Audience = "users";
                    options.TokenValidationParameters.ValidateIssuer = false;
                });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseSwagger(options => options.RouteTemplate = "{documentName}.json");
            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("/openapi.json", "Repost API 0.0.1");
                options.RoutePrefix = "api/swagger";
            });

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseIdentityServer();
            app.UseAuthorization();

            app.UseEndpoints(endpoints => endpoints.MapControllers());
        }
    }
}