// <copyright file="PropertyDataProtection.cs" company="Microsoft">Copyright (c) Microsoft 2015. All rights reserved.</copyright>
namespace Microsoft.Commerce.Payments.PidlModel.V7
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;

    using Newtonsoft.Json;

    public class PropertyDataProtection
    {
        private Dictionary<string, string> protectionParameters;

        public PropertyDataProtection()
        {
        }

        public PropertyDataProtection(PropertyDataProtection template)
        {
            this.ProtectionId = template.ProtectionId;
            this.ProtectionType = template.ProtectionType;

            if (template.protectionParameters != null)
            {
                this.protectionParameters = new Dictionary<string, string>(template.protectionParameters);
            }

            this.ProtectionName = template.ProtectionName;
        }

        public PropertyDataProtection(
            string protectionId,
            string protectionType,
            string protectionName)
        {
            this.ProtectionId = protectionId;
            this.ProtectionType = protectionType;
            this.protectionParameters = new Dictionary<string, string>();
            this.ProtectionName = protectionName;
        }

        public PropertyDataProtection(
            string protectionId,
            string protectionType,
            string protectionName,
            FetchConfig fetchConfig)
        {
            this.ProtectionId = protectionId;
            this.ProtectionType = protectionType;
            this.protectionParameters = new Dictionary<string, string>();
            this.ProtectionName = protectionName;
            this.FetchConfig = fetchConfig;
        }

        [JsonIgnore]
        public string ProtectionId { get; private set; }       

        [JsonProperty(Order = 0, PropertyName = "protection_type")]
        public string ProtectionType { get; set; }

        // If renaming also rename the ShouldSerialize function.
        [JsonProperty(Order = 1, PropertyName = "parameters")]
        [SuppressMessage("Microsoft.Usage", "CA2227", Justification = "Public setter used in serialization.")]
        public Dictionary<string, string> Parameters
        {
            get
            {
                return this.protectionParameters;
            }

            set
            {
                this.protectionParameters = value;
            }
        }

        [JsonProperty(Order = 2, PropertyName = "fetchConfig")]
        public FetchConfig FetchConfig { get; set; }

        [JsonIgnore]
        public string ProtectionName { get; private set; }

        public bool ShouldSerializeParameters()
        {
            return this.Parameters.Count != 0;
        }
    }
}