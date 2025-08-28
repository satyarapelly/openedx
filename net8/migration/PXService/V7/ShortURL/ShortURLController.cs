// <copyright file="ShortURLController.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.V7.ShortURL
{
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Commerce.Payments.Common.Tracing;
    using Microsoft.Commerce.Payments.Common.Web;
    using Microsoft.Commerce.Payments.PXService.Model.ShortURLService;

    [ApiController]
    [Route("{version}/shorturl")]
    public class ShortURLController : ProxyController
    {
        /// <summary>
        /// Creates a short URL for the supplied long URL.
        /// </summary>
        /// <param name="request">Request containing the long URL and optional TTL in minutes.</param>
        /// <returns>The created short URL response.</returns>
        [HttpPost]
        public async Task<ActionResult<CreateShortURLResponse>> Create([FromBody] CreateShortURLRequest request)
        {
            EventTraceActivity traceActivityId = this.Request.GetRequestCorrelationId();
            var response = await this.Settings.ShortURLServiceAccessor.CreateShortURL(request.URL, request.TTLMinutes, traceActivityId);
            return this.Ok(response);
        }

        /// <summary>
        /// Deletes an existing short URL.
        /// </summary>
        /// <param name="codeOrUrl">The short code or URL to delete.</param>
        [HttpDelete("{codeOrUrl}")]
        public async Task<IActionResult> Delete(string codeOrUrl)
        {
            EventTraceActivity traceActivityId = this.Request.GetRequestCorrelationId();
            await this.Settings.ShortURLServiceAccessor.DeleteShortURL(codeOrUrl, traceActivityId);
            return this.NoContent();
        }
    }
}
