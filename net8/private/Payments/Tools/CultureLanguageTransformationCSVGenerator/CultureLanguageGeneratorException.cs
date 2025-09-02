// <copyright file="CultureLanguageGeneratorException.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.Tools.CultureLanguageTransformationCSVGenerator
{
    using System;

    internal class CultureLanguageGeneratorException : Exception
    {
        public CultureLanguageGeneratorException(string message) : base(message)
        {
        }
    }
}
