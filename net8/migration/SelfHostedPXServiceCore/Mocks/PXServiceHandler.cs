// <copyright company="Microsoft Corporation">Copyright (c) Microsoft 2018. All rights reserved.</copyright>

namespace SelfHostedPXServiceCore.Mocks
{
    using System;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Http;

    /// <summary>
    /// Provides hooks for pre- and post-processing around an optional inner handler
    /// without relying on <see cref="DelegatingHandler"/>. Callers can set custom
    /// actions to inspect or modify requests and responses, and choose whether to
    /// invoke the inner handler at all.
    /// </summary>
    public class PXServiceHandler
    {
        private readonly RequestDelegate _next;

        /// <summary>
        /// Action invoked before the request is sent to the inner handler.
        /// </summary>
        public Action<HttpRequestMessage> PreProcess { get; set; }

        /// <summary>
        /// Indicates whether the inner handler should be called.
        /// </summary>
        public bool CallInnerHandler { get; set; }

        /// <summary>
        /// Action invoked after the inner handler returns a response. The returned
        /// <see cref="HttpResponseMessage"/> replaces the inner handler response.
        /// </summary>
        public Func<HttpRequestMessage, HttpResponseMessage, HttpResponseMessage> PostProcess { get; set; }

        public PXServiceHandler()
        {
            ResetToDefault();
        }

        public PXServiceHandler(RequestDelegate next)
            : this()
        {
            _next = next;
        }

        public Task InvokeAsync(HttpContext context)
        {
            return _next != null ? _next(context) : Task.CompletedTask;
        }

        /// <summary>
        /// Resets the handler to its default state.
        /// </summary>
        public void ResetToDefault()
        {
            CallInnerHandler = true;
            PreProcess = null;
            PostProcess = null;
        }

        /// <summary>
        /// Processes the request using the configured actions and optional next delegate.
        /// </summary>
        public async Task<HttpResponseMessage> InvokeAsync(
            HttpRequestMessage request,
            Func<HttpRequestMessage, Task<HttpResponseMessage>> next)
        {
            PreProcess?.Invoke(request);

            HttpResponseMessage response = null;
            if (CallInnerHandler && next != null)
            {
                response = await next(request);
            }

            if (PostProcess != null)
            {
                response = PostProcess(request, response);
            }

            return response;
        }
    }
}

