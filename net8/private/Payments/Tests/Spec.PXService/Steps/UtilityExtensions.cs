// <copyright file="UtilityExtensions.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Spec.PXService.Steps
{
    using System;
    using System.Web;

    public static class UtilityExtensions
    {
        public static bool ToBool(this string value, string positiveValue, string negativeValue)
        {
            if (string.Equals(value, positiveValue, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            if (value == negativeValue)
            {
                return false;
            }

            throw new ArgumentException();
        }

        public static string ToBase64(this string value)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(value);
            return Convert.ToBase64String(plainTextBytes);
        }

        public static string UrlEncode(this string value)
        {
            return HttpUtility.UrlEncode(value);
        }

        public static bool IsNullOrEmpty(this string value)
        {
            return string.IsNullOrEmpty(value);
        }
    }
}