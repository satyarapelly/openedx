// <copyright file="SelfhostHttpContext.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Model
{
    using System.Net.Http;
    using System.Threading;

    /// <summary>
    /// HttpContext becomes null in the selfhosted environment used for running DiffTest due to which Test scenario header doesn't work. 
    /// This class works as alternative to HttpContext to store information as a global context for Selfhosted env.
    /// Context/Request information can be accessed via getters and setters in this class.
    /// </summary>
    public static class SelfhostHttpContext
    {
        private static AsyncLocal<HttpRequestMessage> request = new AsyncLocal<HttpRequestMessage>();
        
        public static HttpRequestMessage Request
        {
            get
            {
                return request.Value;
            }

            set
            {
                request.Value = value;
            }
        }
    }
}