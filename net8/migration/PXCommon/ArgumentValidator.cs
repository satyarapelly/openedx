// <copyright file="ArgumentValidator.cs" company="Microsoft">Copyright (c) Microsoft 2013. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXCommon
{
    using System;
    using Microsoft.Commerce.Payments.Common;

    public static class ArgumentValidator
    {
        public static string EnsureNotNullOrWhitespace(string argument, string argumentName)
        {
            if (string.IsNullOrWhiteSpace(argument))
            {
                throw new ArgumentException("String argument should not be null or white space.", argumentName);
            }

            return argument;
        }

        public static T EnsureNotNull<T>(T argument, string argumentName) where T : class
        {
            if (argument == null)
            {
                throw new ArgumentNullException(argumentName);
            }

            return argument;
        }
    }
}
