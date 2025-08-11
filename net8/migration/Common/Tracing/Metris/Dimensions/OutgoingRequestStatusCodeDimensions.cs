// <copyright file="OutgoingRequestStatusCodeDimensions.cs" company="Microsoft">Copyright (c) Microsoft 2025. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.Common.Tracing.Metris.Dimensions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public class OutgoingRequestStatusCodeDimensions : OutgoingRequestDimensions
    {
        /// <summary>
        /// Gets or sets Http status code of the response for the outgoing request
        /// </summary>
        public string ResponseStatusCode { get; set; }

        /// <summary>
        /// Gets or sets additional code that optionally could be set along with the response
        /// </summary>
        public string SubCode { get; set; }

        /// <summary>
        /// Gets or sets Exception name (System.TaskCancelledException) thrown while processing the outgoing request/response.
        /// </summary>
        public string ExceptionName { get; set; }
    }
}
