// <copyright file="StringExtention.cs" company="Microsoft">Copyright (c) Microsoft 2015. All rights reserved.</copyright>
namespace PidlTest
{
    public static class StringExtention
    {
        public static string Truncate(this string value, int maxLength)
        {
            if (string.IsNullOrEmpty(value))
            {
                return value;
            }

            return value.Length <= maxLength ? value : value.Substring(0, maxLength);
        }
    }
}
