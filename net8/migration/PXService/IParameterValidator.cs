// <copyright file="IParameterValidator.cs" company="Microsoft Corporation">Copyright (c) Microsoft 2020. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService
{
    public interface IParameterValidator
    {
        bool Validate(string paramValue);
    }
}