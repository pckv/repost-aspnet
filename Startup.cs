using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using RepostAspNet.Models;
using Endpoint = IdentityServer4.Hosting.Endpoint;

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
            {
                // Create an in memory database
                // TODO: Use SQL
                options.UseInMemoryDatabase("RepostDatabase");
            });

            services.AddControllers(options =>
                {
                    // Add filter to format all errors (status >= 400) as the ErrorResponse model
                    // This can not handle validation errors. Those are handled in .ConfigureApiBehaviorOptions
                    options.Filters.Add(new ErrorResponseFilter());
                })

                // Registers all controllers as services, which allows for dependency injection
                .AddControllersAsServices()
                .ConfigureApiBehaviorOptions(options =>
                {
                    // Convert default validation errors to 422 using ErrorResponse as a model
                    options.InvalidModelStateResponseFactory = context =>
                    {
                        // Compile a list of formatted strings of all the validation errors for the model
                        var errors = context.ModelState
                            .Where(error => error.Value.Errors.Count > 0)
                            .Select(error =>
                                $"Failed to validate field {error.Key}: {error.Value.Errors.FirstOrDefault()?.ErrorMessage}");

                        return new ObjectResult(new ErrorResponse {Detail = string.Join(", ", errors)})
                        {
                            StatusCode = StatusCodes.Status422UnprocessableEntity
                        };
                    };
                });

            services.AddSwaggerGen(options =>
            {
                // Add main OpenAPI document
                // The name of the document is openapi, in order to name the endpoint /openapi.json
                options.SwaggerDoc("openapi", new OpenApiInfo
                {
                    Title = "Repost",
                    Version = "0.0.1",
                    Description = "Repost API written in ASP.NET Core 3.1 Web APIs\n\n" +
                                  "[View source code on GitHub](https://github.com/pckv/repost-aspnet)\n\n" +
                                  "Authors: pckv, EspenK, jonsondrem"
                });

                // Add OpenAPI security scheme for OAuth2
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
                                {"user", "User access"}
                            }
                        }
                    }
                });

                // Enable use of generated XML document to annotate endpoint name and description in OpenAPI
                var assemblyName = Assembly.GetExecutingAssembly().GetName().Name;
                var path = Path.Combine(AppContext.BaseDirectory, $"{assemblyName}.xml");
                options.IncludeXmlComments(path);

                // Add OpenAPI operation filter to add security schemes to all authorized endpoints automatically
                options.OperationFilter<OpenApiAuthorizationCheckFilter>();
            });

            // Setup authorization and authentication server with a resource owner validator for
            // authorizing database users
            var builder = services.AddIdentityServer()
                .AddDeveloperSigningCredential()
                .AddInMemoryApiResources(Config.Apis)
                .AddInMemoryClients(Config.Clients)
                .AddResourceOwnerValidator<ResourceOwnerPasswordValidator>();

            // Hack to reroute the OpenID endpoint to /api/auth
            // We only require the /token endpoint for our OAuth2 setup, so this hack adjusts the endpoint to
            // match the other implementations 
            builder.Services
                .Where(descriptor => descriptor.ServiceType == typeof(Endpoint))
                .Select(item => (Endpoint) item.ImplementationInstance)
                .ToList()
                .ForEach(item => item.Path = item.Path.Value.Replace("/connect", "/api/auth"));

            // Add internal JSON Web Token validation (issuing tokens is done by IdentityServer4 defined above)
            services.AddAuthentication("Bearer")
                .AddJwtBearer("Bearer", options =>
                {
                    options.Authority = "http://localhost:5000";
                    options.RequireHttpsMetadata = false;
                    options.Audience = "user";
                    options.TokenValidationParameters.ValidateIssuer = false;
                });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            // Use OpenAPI with the route set to {documentName}.json (resolves to openapi.json)
            app.UseSwagger(options => options.RouteTemplate = "{documentName}.json");

            // Use Swagger UI on top of OpenAPI
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