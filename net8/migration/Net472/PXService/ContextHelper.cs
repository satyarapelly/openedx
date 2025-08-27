// <copyright file="ContextHelper.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Security.Cryptography;
    using Common.Web;
    using Microsoft.Commerce.Payments.Common.Tracing;
    using Microsoft.Commerce.Payments.PXService.Settings;

    public class ContextHelper
    {
        // Salt used to hash xboxLiveDeviceId for PayPal
        private static readonly byte[] HashSalt = new Guid("29A73283-3B37-458F-9866-95AFF6CDF3A0").ToByteArray();

        /// <summary>
        /// Returns the context value of a specified contextKey in the given input context
        /// </summary>
        /// <param name="context">Context dictionary to retreive the value from</param>
        /// <param name="contextPath">Context path is a dot-separate path to the value to be retrieved. 
        /// Example: "MsaProfile.emailAddress"
        /// </param>
        /// <returns>Returns the context value or null if not found.</returns>
        public static string TryGetContextValue(Dictionary<string, object> context, string contextPath)
        {
            string retVal = null;
            string[] contextKeys = contextPath.Split(new char[] { '.' });
            if (contextKeys.Length != 2)
            {
                throw new ArgumentException("Currently, only 2 levels are supported in the context dictionary");
            }

            Dictionary<string, string> contextGroup;
            if (context.ContainsKey(contextKeys[0]))
            {
                contextGroup = context[contextKeys[0]] as Dictionary<string, string>;
                if (contextGroup != null)
                {
                    contextGroup.TryGetValue(contextKeys[1], out retVal);
                }
            }

            return retVal;
        }

        public static void GetContext(HttpRequestMessage request, PXServiceSettings settings, ref Dictionary<string, object> context)
        {
            GetClientContext(request, ref context);
            GetServiceContext(settings, ref context);
        }

        /// <summary>
        /// This function parses client context headers and adds them to the clientContext dictionary.  If the clientContext is null, 
        /// it will be instantiated.  
        /// <para> 
        /// Client context headers are expected to be as shown in the below example: 
        /// Headers["x-ms-deviceinfo"] = "ipAddress=131.107.174.30,xboxLiveDeviceId=18230582934452242973"
        /// If GlobalConstants.ClientContextTypes["x-ms-deviceinfo"] = "DeviceInfo", the above example will lead 
        /// to 2 entries being written to the clientContext dictionary as below:
        ///     clientContext["DeviceInfo.ipAddress"] = "131.107.174.30"
        ///     clientContext["DeviceInfo.xboxLiveDeviceId"] = "18230582934452242973"
        /// </para>
        /// <para>
        /// If a client context header does not exist in the incoming request headers, clientContext is not modified.
        /// If this function finds a client context that already exists in the clientContext, the value will be overwritten.
        /// </para>
        /// </summary>
        /// <param name="request">HttpRequestMessage object to extract the context from</param>
        /// <param name="context">Context dictionary to which context groups and values need to be added</param>
        public static void GetClientContext(HttpRequestMessage request, ref Dictionary<string, object> context)
        {
            if (context == null)
            {
                context = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
            }

            if (request == null || request.Headers == null)
            {
                return;
            }

            bool isEncoded = false;
            IEnumerable<string> encoding = null;
            if (request.Headers.TryGetValues(GlobalConstants.HeaderValues.ClientContextEncoding, out encoding))
            {
                string clientContextEncoding = encoding.FirstOrDefault();
                if (!string.IsNullOrEmpty(clientContextEncoding))
                {
                    isEncoded = true;
                }
            }

            foreach (string headerName in GlobalConstants.ClientContextGroups.FromHeader.Keys)
            {
                string headerValue = null;
                IEnumerable<string> headerValues = null;
                if (request.Headers.TryGetValues(headerName, out headerValues))
                {
                    headerValue = headerValues.FirstOrDefault();
                }

                if (headerValue == null)
                {
                    continue;
                }

                string groupName = GlobalConstants.ClientContextGroups.FromHeader[headerName];
                Dictionary<string, string> contextGroup = null;
                object result = null;
                context.TryGetValue(groupName, out result);
                contextGroup = result as Dictionary<string, string>;
                if (contextGroup == null)
                {
                    contextGroup = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                    context[groupName] = contextGroup;
                }

                headerValue = headerValue.Trim('"');
                string[] keyValuePairs = headerValue.Split(',');
                foreach (string keyValuePair in keyValuePairs)
                {
                    int indexOfDelim = keyValuePair.IndexOf('=');
                    if (indexOfDelim > -1)
                    {
                        string key = keyValuePair.Substring(0, indexOfDelim).Trim();
                        string value = keyValuePair.Substring(indexOfDelim + 1);

                        if (isEncoded)
                        {
                            value = ContextHelper.Base64Decode(value, request.GetRequestCorrelationId()); 
                        }

                        // If key is xboxLiveDeviceId, hash device id
                        if (string.Compare(key, GlobalConstants.DeviceIdNames.XboxLiveDeviceId, StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            value = ContextHelper.HashWithSalt(value);
                        }

                        contextGroup[key] = value;
                    }
                }
            }
        }

        public static void GetServiceContext(PXServiceSettings settings, ref Dictionary<string, object> context)
        {
            if (context == null)
            {
                context = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
            }

            Dictionary<string, string> contextGroup;
            object value = null;
            if (context.TryGetValue(PidlFactory.GlobalConstants.ServiceContextGroups.Pifd, out value))
            {
                contextGroup = value as Dictionary<string, string>;
            }
            else
            {
                contextGroup = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                context[PidlFactory.GlobalConstants.ServiceContextGroups.Pifd] = contextGroup;
            }

            contextGroup[PidlFactory.GlobalConstants.ServiceContextKeys.BaseUrl] = settings.PifdBaseUrl;
        }

        private static string Base64Decode(string encodedString, Tracing.EventTraceActivity traceActivityId)
        {
            string retVal = string.Empty;
            try
            {
                var decodedData = Convert.FromBase64String(encodedString);
                retVal = System.Text.Encoding.UTF8.GetString(decodedData);
            }
            catch (Exception ex)
            {
                SllWebLogger.TracePXServiceException(ex.ToString(), traceActivityId);
            }

            return retVal;
        }

        private static string HashWithSalt(string input)
        {
            var inputData = System.Text.Encoding.UTF8.GetBytes(input);
            byte[] message = new byte[inputData.Length + HashSalt.Length];
            inputData.CopyTo(message, 0);
            HashSalt.CopyTo(message, inputData.Length);

            byte[] derivedData = SHA256.Create().ComputeHash(message);
            byte[] derivedDataFirstHalf = new byte[derivedData.Length / 2];
            Buffer.BlockCopy(derivedData, 0, derivedDataFirstHalf, 0, derivedDataFirstHalf.Length);

            return BitConverter.ToString(derivedDataFirstHalf).Replace("-", string.Empty);
        }
    }
}