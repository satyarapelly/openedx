// <copyright file="DynamicConfigurationContext.cs" company="Microsoft">Copyright (c) Microsoft 2017. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.Common.Transaction
{
    using System.Collections.Generic;

    public class DynamicConfigurationContext
    {
        public DynamicConfigurationContext()
        {
            this.ResultContext = new Dictionary<string, object>();
            this.SupportedProvidersList = new List<string>();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227", Justification = "Needed to be set.")]
        public List<string> SupportedProvidersList { get; set; }

        public string ProviderName { get; set; }

        public string Id { get; set; }

        public Dictionary<string, object> ResultContext { get; private set; }
    }
}
