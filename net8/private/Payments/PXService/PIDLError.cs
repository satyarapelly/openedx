// <copyright file="PIDLError.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using Newtonsoft.Json;

    public class PIDLError
    {
        public PIDLError()
        {
        }

        /// <summary>
        /// Gets or sets the error code 
        /// </summary>
        [JsonProperty(PropertyName = "code")]
        public string Code { get; set; }

        /// <summary>
        /// Gets or sets details about the error. This lists all possible reasons for the failure.
        /// e.g. If multiple attributes in the input payload is invalid, they would be listed here.
        /// </summary>
        [JsonProperty(PropertyName = "details")]
        [SuppressMessage("Microsoft.Usage", "CA2227", Justification = "Allowed to be set by derived classes")]
        public List<PIDLErrorDetail> Details { get; set; }

        /// <summary>
        /// Initializes the Details list if needed and adds the given PIDLErrorDetail to it
        /// </summary>
        /// <param name="newDetail">A new detail object that needs to be added to the list</param>
        public void AddDetail(PIDLErrorDetail newDetail)
        {
            if (this.Details == null)
            {
                this.Details = new List<PIDLErrorDetail>();
            }

            this.Details.Add(newDetail);
        }
    }
}