// <copyright file="InstrumentManagementServiceErrorResponse.cs" company="Microsoft">Copyright (c) Microsoft 2019. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PimsModel.V4
{
    using System.Collections.Generic;

    public class InstrumentManagementServiceErrorResponse : ErrorResponseResource
    {
        private IList<InstrumentManagementServiceErrorResponse> details = new List<InstrumentManagementServiceErrorResponse>();

        public string CorrelationId { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1819", Justification = "Needed for error response.")]
        public string[] Targets { get; set; }

        public IList<InstrumentManagementServiceErrorResponse> Details
        {
            get
            {
                return this.details;
            }
        }
    }
}