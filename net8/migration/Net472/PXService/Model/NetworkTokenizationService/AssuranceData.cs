// <copyright file="AssuranceData.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Model.NetworkTokenizationService
{
    /// <summary>
    /// Represents AssuranceData.
    /// </summary>
    public class AssuranceData
    {
        /// <summary>
        /// Gets or sets the FidoBlob.
        /// </summary>
        public string FidoBlob { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the rp id.
        /// </summary>
        public string RpID { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the identifier.
        /// </summary>
        public string Identifier { get; set; } = string.Empty;
    }
}