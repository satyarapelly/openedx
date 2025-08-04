// <copyright file="AddHeaderParameterOperationFilter.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.Tests.Emulators.PXDependencyEmulators
{
    using System.Collections.Generic;
    using Microsoft.OpenApi.Models;
    using Swashbuckle.AspNetCore.SwaggerGen;

    public class AddHeaderParameterOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            operation.Parameters ??= new List<OpenApiParameter>();

            operation.Parameters.Add(new OpenApiParameter
            {
                Name = "x-ms-test",
                In = ParameterLocation.Header,
                Description = "Send x-ms-test header value",
                Required = false,
                Schema = new OpenApiSchema { Type = "string" }
            });
        }
    }
}
