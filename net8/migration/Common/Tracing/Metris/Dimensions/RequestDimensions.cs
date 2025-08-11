// <copyright file="RequestDimensions.cs" company="Microsoft">Copyright (c) Microsoft 2025. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.Common.Tracing.Metris.Dimensions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public class RequestDimensions
    {
        /// <summary>
        /// Gets or sets the API name.
        /// </summary>
        public string OperationName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets an additional code that optionally could be set along with the response.
        /// </summary>
        public string ResponseStatusCode { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the caller name.
        /// </summary>
        public string CallerName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the Qos service request status.
        /// </summary>
        public string RequestStatus { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the scenario id.
        /// </summary>
        public string Scenario { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the Partner.
        /// </summary>
        public string Partner { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the PaymentMethodFamily.
        /// </summary>
        public string PaymentMethodFamily { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the PaymentMethodType.
        /// </summary>
        public string PaymentMethodType { get; set; } = string.Empty;
    }
}