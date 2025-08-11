// <copyright file="HashHelper.cs" company="Microsoft">Copyright (c) Microsoft 2015. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.Common.Helper
{
    using System.Security.Cryptography;
    using System.Text;

    public static class HashHelper
    {
        public static string GetSha256Hash(string value)
        {
            StringBuilder sb = new StringBuilder();
            using (SHA256 hash = SHA256Managed.Create())
            {
                byte[] result = hash.ComputeHash(Encoding.UTF8.GetBytes(value));

                foreach (byte b in result)
                {
                    sb.Append(b.ToString("x2"));
                }
            }

            return sb.ToString();
        }
    }
}
