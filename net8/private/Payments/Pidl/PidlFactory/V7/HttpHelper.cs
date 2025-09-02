// <copyright file="HttpHelper.cs" company="Microsoft Corporation">Copyright (c) Microsoft 2015. All rights reserved.</copyright>
namespace Microsoft.Commerce.Payments.PidlFactory.V7
{
    using System.Diagnostics;
    using System.Net;
    using System.Web;
    using PXCommon;

    internal static class HttpHelper
    {
        public static HttpWebResponse SendAndReceive(HttpWebRequest request)
        {
            HttpWebResponse response = null;
            int tryCount = 0;
            bool retryableError = false;
            do
            {
                tryCount++;
                retryableError = false;
                try
                {
                    // Set proxy to localhost:8888 to view traffic in fiddler (only on a dev environment) 
                    // request.Proxy = new WebProxy(@"http://localhost:8888/"); // lgtm[cs/non-https-url] Suppressing Semmle warning
                    response = (HttpWebResponse)request.GetResponse();
                    Trace.TraceInformation("Response Status | ({0}-{1})", (int)response.StatusCode, response.StatusCode);
                    
                    // GetResponse did not throw an exception means that the network call was successful and also that the Http
                    // response is of type 2XX.  Deserialize the payload to the TResSuccess type
                }
                catch (WebException webEx)
                {
                    // GetResponse throws a WebException on failure. 3XX is treated as a failure too.
                    if (webEx.Status == WebExceptionStatus.ProtocolError)
                    {
                        // Status is ProtocolError means that the network call completed but there was a failure at the 
                        // Http layer (i.e. Http Response status is not 2XX).  The response may contain a payload.
                        response = (HttpWebResponse)webEx.Response;
                    }
                    else
                    {
                        if (tryCount < Constants.RetryCount.MaxRetryCountOnNetworkError)
                        {
                            retryableError = true;
                        }
                        else
                        {
                            throw;
                        }
                    }
                }
            } 
            while (retryableError == true);

            return response;
        }
    }
}
