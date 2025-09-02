// <copyright file="LogDataFilter.cs" company="Microsoft">Copyright (c) Microsoft 2017. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.Common.Tracing
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Text.RegularExpressions;

    public static class LogDataFilter
    {
        public static string KeyValueLogMessageFiltering(string message, HashSet<string> sensitiveKeys)
        {
            try
            {
                if (!string.IsNullOrEmpty(message))
                {
                    StringBuilder sb = new StringBuilder();
                    string[] sections = message.Split('&');
                    for (int i = 0; i < sections.Length; i++)
                    {
                        string[] keyValue = sections[i].Split('=');
                        if (keyValue.Length > 1 && sensitiveKeys.Contains(keyValue[0]) && !string.IsNullOrEmpty(keyValue[1]))
                        {
                            sb.Append(keyValue[0] + "=masked");
                        }
                        else
                        {
                            sb.Append(sections[i]);
                        }

                        if (i < sections.Length - 1)
                        {
                            sb.Append("&");
                        }
                    }

                    return sb.ToString();
                }
                else
                {
                    return message;
                }
            }
            catch
            {
                return message;
            }
        }

        public static string XmlNodeContentMasking(string message, string nodeName)
        {
            if (!string.IsNullOrEmpty(message))
            {
                try
                {
                    string matchPattern = string.Format("<{0}>(.|\\s)*</{0}>", nodeName);
                    string replaceString = string.Format("<{0}>****</{0}>", nodeName);
                    Regex rgx = new Regex(matchPattern);
                    return rgx.Replace(message, replaceString);
                }
                catch
                {
                }
            }

            return message;
        }
    }
}
