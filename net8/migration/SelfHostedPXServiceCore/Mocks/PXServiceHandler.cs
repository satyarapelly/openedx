// <copyright company="Microsoft Corporation">Copyright (c) Microsoft 2018. All rights reserved.</copyright>

namespace SelfHostedPXServiceCore.Mocks
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Http;

    /// <summary>
    /// Provides hooks for pre- and post-processing around the request pipeline
    /// without relying on <see cref="DelegatingHandler"/>.
    /// </summary>
    public class PXServiceHandler : IMiddleware
    {
        /// <summary>
        /// Action invoked before the request is sent to the next middleware.
        /// </summary>
        public Action<HttpContext> PreProcess { get; set; }

        /// <summary>
        /// Indicates whether the next middleware should be executed.
        /// </summary>
        public bool CallInnerHandler { get; set; }

        /// <summary>
        /// Action invoked after the next middleware completes.
        /// </summary>
        public Func<HttpContext, Task> PostProcess { get; set; }

        public PXServiceHandler()
        {
            ResetToDefault();
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
        /// Middleware entry point used by ASP.NET Core.
        /// </summary>
        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            PreProcess?.Invoke(context);

            if (CallInnerHandler && next != null)
            {
                await next(context);
            }

            if (PostProcess != null)
            {
                await PostProcess(context);
            }
        }
    }
}

