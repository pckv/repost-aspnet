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
            if (!context.ApiDescription.CustomAttributes().OfType<AuthorizeAttribute>().Any())
            {
                return;
            }

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

            operation.Responses.Add("401", new OpenApiResponse {Description = "Unauthorized", Content = content});
            operation.Responses.Add("403", new OpenApiResponse {Description = "Forbidden", Content = content});
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