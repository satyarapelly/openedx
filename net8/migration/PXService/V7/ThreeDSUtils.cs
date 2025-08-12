// <copyright file="ThreeDSUtils.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.V7
{
    using System;
    using System.Text;
    using Newtonsoft.Json;

    public class ThreeDSUtils
    {
        public static string EncodeObjectToBase64(object value)
        {
            return value == null ? null : EncodeToBase64(JsonConvert.SerializeObject(
                        value,
                        new JsonSerializerSettings
                        {
                            NullValueHandling = NullValueHandling.Ignore
                        }));
        }

        public static string EncodeToBase64(string value)
        {
            return value == null ? value : Convert.ToBase64String(Encoding.ASCII.GetBytes(value));
        }

        public static string DecodeBase64(string value)
        {
            return value == null ? value : Encoding.ASCII.GetString(Convert.FromBase64String(value));
        }

        public static string EncodeUrl(string base64Val)
        {
            if (base64Val == null)
            {
                throw new ArgumentNullException("base64Val");
            }

            return base64Val
                .Replace("=", string.Empty)
                .Replace("/", "_")
                .Replace("+", "-");
        }

        public static string DecodeUrl(string arg)
        {
            if (arg == null)
            {
                throw new ArgumentNullException("arg");
            }

            var base64val = arg
                    .PadRight(arg.Length + ((4 - (arg.Length % 4)) % 4), '=')
                    .Replace("_", "/")
                    .Replace("-", "+");

            return base64val;
        }
    }
}