// <copyright file="InlineSessionDataParser.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Model.SessionService
{
    using System;
    using System.Text.RegularExpressions;

    public class InlineSessionDataParser
    {
        // Regex to match session Id with inline data. Refer to this RFC : https://www.ietf.org/rfc/rfc4648.txt for Base64Encoding
        private static readonly Regex SessionIdPatternRegex = new Regex(
            @"(ByRef|ByVal)(:)((?:[az09]*))(:)(\d+)(\.)(\d+)(:)([azAZ09/+]*={0,2})",
            RegexOptions.Singleline | RegexOptions.IgnoreCase);

        /// <summary>
        /// This function checks if the session Id is in the format that defines inline session data(e.g: ByVal:Type:Version:Base64EncodedString"
        /// The Base64EncodedString is encoded json which contains inline data.
        /// </summary>
        /// <param name="sessionId">session Id</param>
        /// <returns>session Id inline data and passing mechanism if found.</returns>
        public static string GetSessionInlineData(string sessionId)
        {
            if (string.IsNullOrEmpty(sessionId))
            {
                throw new ArgumentException("Session Id is null");
            }

            var match = SessionIdPatternRegex.Match(sessionId);

            if (match.Success)
            {
                var passingMechanism = match.Groups[1].ToString();

                var encodedInlineDataJson = match.Groups[9].ToString();

                if (string.Equals(passingMechanism, "ByVal", StringComparison.OrdinalIgnoreCase))
                {
                    var base64EncodedBytes = Convert.FromBase64String(encodedInlineDataJson);
                    var decodedJson = System.Text.Encoding.UTF8.GetString(base64EncodedBytes);

                    return decodedJson;
                }

                throw new NotImplementedException("ByRef is not implemented.");
            }

            return null;
        }
    }
}