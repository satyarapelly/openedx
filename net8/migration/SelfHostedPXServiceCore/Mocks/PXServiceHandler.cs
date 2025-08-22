// <copyright company="Microsoft Corporation">Copyright (c) Microsoft 2018. All rights reserved.</copyright>

namespace SelfHostedPXServiceCore.Mocks
{
    using System;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// This is a delegating handler that allows callers to configure its behavior by setting custom actions
    /// to be executed before and after calling the next handler.  It also allows callers to determine if 
    /// the next hander should be called at all or if a configurable default response should be returned thus
    /// short-circuiting the pipeline.
    /// </summary>
    public class PXServiceHandler : DelegatingHandler
    {
        /// <summary>
        /// This action is called before sending the request to the next in the pipeline.
        /// </summary>
        public Action<HttpRequestMessage> PreProcess { get; set; }

        /// <summary>
        /// Determines if the request should be sent to the next in the pipeline.
        /// </summary>
        public bool CallInnerHandler { get; set; }

        /// <summary>
        /// This action is performed after the the pipeline returns the response. It is
        /// only performed if ProcessRemaining is set to true.
        /// </summary>
        public Func<HttpRequestMessage, HttpResponseMessage, HttpResponseMessage> PostProcess { get; set; }

        public PXServiceHandler()
        {
            ResetToDefault();
        }

        public void ResetToDefault()
        {
            CallInnerHandler = true;
            PreProcess = null;
            PostProcess = null;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (PreProcess != null)
            {
                PreProcess(request);
            }

            HttpResponseMessage response = null;
            if (CallInnerHandler)
            {
                response = await base.SendAsync(request, cancellationToken);
            }

            if (PostProcess != null)
            {
                response = PostProcess(request, response);
            }

            return response;
        }
    }
}
