using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Collections.Generic;
using System.Linq;

namespace QuanLyResort.Filters;

/// <summary>
/// Operation filter để xử lý file upload (IFormFile) trong Swagger
/// </summary>
public class FileUploadOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        // Check if this operation has any IFormFile parameters
        var fileParameters = context.ApiDescription.ParameterDescriptions
            .Where(p => 
            {
                var paramType = p.Type;
                // Check for IFormFile (non-nullable) or nullable IFormFile
                if (paramType == typeof(IFormFile))
                    return true;
                
                // Check for nullable IFormFile?
                if (paramType.IsGenericType && 
                    paramType.GetGenericTypeDefinition() == typeof(Nullable<>) && 
                    paramType.GetGenericArguments()[0] == typeof(IFormFile))
                    return true;
                
                return false;
            })
            .ToList();

        if (!fileParameters.Any())
            return;

        // Clear all existing parameters (they will be replaced by request body)
        operation.Parameters.Clear();

        // Build request body schema with all file parameters
        var schemaProperties = new Dictionary<string, OpenApiSchema>();
        var requiredProperties = new HashSet<string>();

        foreach (var fileParam in fileParameters)
        {
            schemaProperties[fileParam.Name] = new OpenApiSchema
            {
                Type = "string",
                Format = "binary",
                Description = "File to upload"
            };
            
            // Add to required if non-nullable
            if (fileParam.Type == typeof(IFormFile))
            {
                requiredProperties.Add(fileParam.Name);
            }
        }

        // Also include route parameters (like {id}) in the operation
        var routeParams = context.ApiDescription.ParameterDescriptions
            .Where(p => p.Source == Microsoft.AspNetCore.Mvc.ModelBinding.BindingSource.Path)
            .ToList();
        
        foreach (var routeParam in routeParams)
        {
            operation.Parameters.Add(new OpenApiParameter
            {
                Name = routeParam.Name,
                In = ParameterLocation.Path,
                Required = true,
                Schema = new OpenApiSchema
                {
                    Type = routeParam.Type == typeof(int) ? "integer" : "string"
                }
            });
        }

        // Create multipart/form-data media type
        var uploadFileMediaType = new OpenApiMediaType
        {
            Schema = new OpenApiSchema
            {
                Type = "object",
                Properties = schemaProperties,
                Required = requiredProperties.Any() ? requiredProperties : null
            }
        };

        // Set request body
        operation.RequestBody = new OpenApiRequestBody
        {
            Content = new Dictionary<string, OpenApiMediaType>
            {
                ["multipart/form-data"] = uploadFileMediaType
            },
            Required = requiredProperties.Any()
        };
    }
}
