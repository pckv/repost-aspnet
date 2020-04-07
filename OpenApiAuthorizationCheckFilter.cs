using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace RepostAspNet
{
    public class OpenApiAuthorizationCheckFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            // Only match endpoints attributed with [Authorize]
            if (!context.ApiDescription.CustomAttributes().OfType<AuthorizeAttribute>().Any())
            {
                return;
            }

            // Content with the response schema as a reference to ErrorResponse
            var content = new Dictionary<string, OpenApiMediaType>
            {
                {
                    "application/json", new OpenApiMediaType
                    {
                        Schema = new OpenApiSchema
                        {
                            Reference = new OpenApiReference {Type = ReferenceType.Schema, Id = "ErrorResponse"}
                        }
                    }
                }
            };

            // Add 400 and 401 to the possible responses
            if (!operation.Responses.ContainsKey("400"))
                operation.Responses.Add("400", new OpenApiResponse {Description = "Bad Request", Content = content});

            if (!operation.Responses.ContainsKey("401"))
                operation.Responses.Add("401", new OpenApiResponse {Description = "Unauthorized", Content = content});

            // Add a reference to OAuth2 as the security scheme required for the endpoint
            operation.Security = new List<OpenApiSecurityRequirement>
            {
                new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference {Type = ReferenceType.SecurityScheme, Id = "oauth2"}
                        },
                        new List<string> {"users"}
                    }
                }
            };
        }
    }
}