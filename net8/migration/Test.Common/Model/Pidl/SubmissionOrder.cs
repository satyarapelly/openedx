// <copyright file="SubmissionOrder.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Tests.Common.Model.Pidl
{
    using Newtonsoft.Json;

    public class SubmissionOrder
    {
        public SubmissionOrder()
        {
        }

        /// <summary>
        /// Gets or sets name of the PIDL instance
        /// </summary>
        [JsonProperty(PropertyName = "instanceName")]
        public string InstanceName { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether true execute the submit action for validation only, its not going to submit the data.
        /// </summary>
        [JsonProperty(PropertyName = "validateOnly")]
        public bool ValidateOnly { get; set; }
    }
}