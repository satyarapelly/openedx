// <copyright file="ParameterRegexValidator.cs" company="Microsoft Corporation">Copyright (c) Microsoft 2020. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService
{
    using System.Text.RegularExpressions;

    public class ParameterRegexValidator : IParameterValidator
    {
        private Regex validPattern;

        public ParameterRegexValidator(string validPattern)
        {
            this.validPattern = new Regex(validPattern, RegexOptions.IgnoreCase);
        }

        public bool Validate(string paramValue)
        {
            return this.validPattern.IsMatch(paramValue);
        }
    }
}