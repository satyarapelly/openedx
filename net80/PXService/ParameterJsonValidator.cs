// <copyright file="ParameterJsonValidator.cs" company="Microsoft Corporation">Copyright (c) Microsoft 2020. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService
{
    using System;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    public class ParameterJsonValidator : IParameterValidator
    {
        public bool Validate(string paramValue)
        {
            paramValue = paramValue.Trim();
            if ((paramValue.StartsWith("{") && paramValue.EndsWith("}")) || 
                (paramValue.StartsWith("[") && paramValue.EndsWith("]"))) 
            {
                try
                {
                    var obj = JToken.Parse(paramValue);
                    return true;
                }
                catch (Exception) 
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }
    }
}