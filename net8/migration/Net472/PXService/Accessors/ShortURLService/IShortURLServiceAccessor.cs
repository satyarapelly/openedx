// <copyright file="IShortURLServiceAccessor.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Accessors.ShortURLService
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.Commerce.Payments.PXService.Model.ShortURLService;
    using Microsoft.Commerce.Tracing;
    
    public interface IShortURLServiceAccessor
    {
        /// <summary>
        /// Creates a ShortURL
        /// </summary>
        /// <param name="longUrl">longUrl to be accessed using a shortUrl</param>
        /// <param name="ttlMinutes">Optional time to live.</param>
        /// <param name="traceActivityId">Trace activity Id.</param>
        /// <returns>the ShortURL</returns>
        Task<CreateShortURLResponse> CreateShortURL(string longUrl, int? ttlMinutes, EventTraceActivity traceActivityId);

        /// <summary>
        /// Invalidates an Existing ShortURL
        /// </summary>
        /// <param name="codeOrUrl">Either the shortUrl code or the longUrl associated with the shortUrl.</param>
        /// <param name="traceActivityId">Trace activity Id.</param>
        /// <returns>No content.</returns>
        Task DeleteShortURL(string codeOrUrl, EventTraceActivity traceActivityId);
    }
}