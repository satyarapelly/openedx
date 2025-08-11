// <copyright file="HttpStatusCodeExtensions.cs" company="Microsoft Corporation">Copyright (c) Microsoft 2014. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.Common.Web
{
    using System.Net;

    public static class HttpStatusCodeExtensions
    {
        public static bool IsRetryable(this HttpStatusCode statusCode)
        {
            if ((statusCode >= (HttpStatusCode)400 && statusCode < (HttpStatusCode)500) ||
                statusCode == (HttpStatusCode)501 ||
                statusCode == (HttpStatusCode)505)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Verifies if the error code is a server error, in which case the request would be counted as failure.
        /// </summary>
        /// <param name="code">Http status code</param>
        /// <returns>True if the error a server error</returns>
        public static bool IsServerError(this HttpStatusCode code)
        {
            int responseCodeNumber = (int)code;

            // 5xx is for Server errors
            return 500 <= responseCodeNumber && responseCodeNumber <= 599;
        }

        public static bool IsClientError(this HttpStatusCode code)
        {
            int responseCodeNumber = (int)code;
            return 400 <= responseCodeNumber && responseCodeNumber < 500;
        }

        public static bool IsSuccessStatus(this HttpStatusCode code)
        {
            int responseCodeNumber = (int)code;
            return 200 <= responseCodeNumber && responseCodeNumber < 300;
        }
    }
}
