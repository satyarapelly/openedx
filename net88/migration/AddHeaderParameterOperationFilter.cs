// <copyright file="AddHeaderParameterOperationFilter.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.Tests.Emulators.PXDependencyEmulators
{
    using System.Collections.Generic;
    using System.Web.Http.Description;
    using Swashbuckle.Swagger;

    public class AddHeaderParameterOperationFilter : IOperationFilter
    {
        public void Apply(Operation operation, SchemaRegistry schemaRegistry, ApiDescription apiDescription)
        {
            if (operation.parameters == null)
            {
                operation.parameters = new List<Parameter>();
            }

            operation.parameters.Add(new Parameter
            {
                name = "x-ms-test",
                @in = "header",
                description = "Send x-ms-test header value",
                required = false,
                type = "string"
            });
        }
    }
}