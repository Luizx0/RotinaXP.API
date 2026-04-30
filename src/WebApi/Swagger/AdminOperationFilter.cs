using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace RotinaXP.API.WebSwagger;

public class AdminOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var path = context.ApiDescription.RelativePath ?? string.Empty;
        if (path.StartsWith("admin/", StringComparison.OrdinalIgnoreCase) || path.StartsWith("admin/", StringComparison.OrdinalIgnoreCase))
        {
            operation.Parameters ??= new List<OpenApiParameter>();

            operation.Parameters.Add(new OpenApiParameter
            {
                Name = "X-User-Role",
                In = ParameterLocation.Header,
                Required = false,
                Description = "DEV ONLY: Set to 'Admin' to simulate an admin role. In production use a JWT with role=Admin.",
                Schema = new OpenApiSchema { Type = "string" }
            });

            // Add note to description
            var note = "\n**Admin endpoints**: require authentication with a token where the user has role=Admin. In development you may set header X-User-Role=Admin.";
            operation.Description = (operation.Description ?? string.Empty) + note;
        }
    }
}
