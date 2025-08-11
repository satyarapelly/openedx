// <copyright file="OutgoingRequestDimensions.cs" company="Microsoft">Copyright (c) Microsoft 2025. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.Common.Tracing.Metris.Dimensions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public class OutgoingRequestDimensions
    {
        /// <summary>
        /// Gets or sets name of the dependency
        /// </summary>
        public string DependencyName { get; set; }

        /// <summary>
        /// Gets or sets an indication whether the error is retriable or not
        /// </summary>
        public bool? IsRetriableError { get; set; }

        /// <summary>
        /// Gets or sets name of the Operation
        /// </summary>
        public string OperationName { get; set; }

        /// <summary>
        /// Gets or sets version of the operation
        /// </summary>
        public string OperationVersion { get; set; }
    }
}
