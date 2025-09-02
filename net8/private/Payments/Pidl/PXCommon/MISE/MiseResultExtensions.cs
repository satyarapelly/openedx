// <copyright file="MiseResultExtensions.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXCommon
{
    using System.Collections.Generic;
    using System.Text;
    using Microsoft.Identity.ServiceEssentials;
    using Microsoft.IdentityModel.S2S;

    /// <summary>
    /// Defines the <see cref="MiseResultExtensions" />.
    /// </summary>
    public static class MiseResultExtensions
    {
        /// <summary>
        /// Extract S2SAuthenticationResult from MiseResult to get instance name which is used to successfully validate the aad token.
        /// </summary>
        /// <param name="miseResult">The miseResult<see cref="MiseResult{MiseHttpContext}"/>.</param>
        /// <returns>The instance name.</returns>
        public static string GetInstanceName(this MiseResult<MiseHttpContext> miseResult)
        {
            string instanceName = null;
            if (miseResult.MiseContext.PropertyBag.ContainsKey("Microsoft.IdentityModel.S2S.S2SAuthenticationResult"))
            {
                var s2sAuthenticationResult = (S2SAuthenticationResult)miseResult.MiseContext.PropertyBag["Microsoft.IdentityModel.S2S.S2SAuthenticationResult"];
                instanceName = s2sAuthenticationResult?.Ticket?.InboundPolicy?.Label ?? instanceName;
            }

            return instanceName;
        }

        /// <summary>
        /// Get additional http response information from MiseResult.
        /// </summary>
        /// <param name="miseResult">The miseResult<see cref="MiseResult{MiseHttpContext}"/>.</param>
        /// <returns>The additional information.</returns>
        public static string GetAdditionalHttpResponseInformation(this MiseResult<MiseHttpContext> miseResult)
        {
            string message = null;
            if (miseResult?.MiseContext?.HttpSuccessfulResponseBuilder != null)
            {
                var additionalHttpResponseParams = miseResult.MiseContext.HttpSuccessfulResponseBuilder.BuildResponse(200);
                var additionalInfo = new List<string>();
                foreach (var header in additionalHttpResponseParams.Headers)
                {
                    additionalInfo.Add($"Header - key:{header.Key} value:{string.Join(",", header.Value)}");
                }

                if (additionalInfo.Count > 0)
                {
                    message = string.Join(";", additionalInfo);
                }
            }

            return message;
        }

        /// <summary>
        /// Get module failure response information from MiseResult.
        /// </summary>
        /// <param name="miseResult">The miseResult<see cref="MiseResult{MiseHttpContext}"/>.</param>
        /// <returns>The failure information.</returns>
        public static string GetModuleFailureResponseInformation(this MiseResult<MiseHttpContext> miseResult)
        {
            string message = null;
            if (miseResult?.MiseContext?.ModuleFailureResponse != null)
            {
                var moduleCreatedFailureResponse = miseResult.MiseContext.ModuleFailureResponse;
                var additionalInfo = new List<string>();
                foreach (var header in moduleCreatedFailureResponse.Headers)
                {
                    additionalInfo.Add($"Header - key:{header.Key} value:{string.Join(",", header.Value)}");
                }

                if (additionalInfo.Count > 0)
                {
                    message = string.Join(";", additionalInfo);
                }

                if (moduleCreatedFailureResponse.Body != null)
                {
                    message += $"HTTP Body: {Encoding.UTF8.GetString(moduleCreatedFailureResponse.Body, 0, moduleCreatedFailureResponse.Body.Length)}";
                }
            }

            return message;
        }
    }
}