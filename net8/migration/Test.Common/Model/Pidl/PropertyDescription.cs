// <copyright file="PropertyDescription.cs" company="Microsoft">Copyright (c) Microsoft 2015. All rights reserved.</copyright>

namespace Tests.Common.Model.Pidl
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using Newtonsoft.Json;

    public class PropertyDescription
    {
        [JsonProperty(Order = 0, PropertyName = "propertyType")]
        public string PropertyType { get; set; }

        // Kept for backward compatibility reasons. To be deprecated
        [JsonProperty(Order = 1, PropertyName = "type")]
        public string PropertyDescriptionType { get; set; }

        [JsonProperty(Order = 2, PropertyName = "dataType")]
        public string DataType { get; set; }

        [JsonProperty(Order = 3, PropertyName = "is_id")]
        public bool? IsId { get; set; }

        [JsonProperty(Order = 4, PropertyName = "is_key")]
        public bool? IsKey { get; set; }

        [JsonProperty(Order = 5, PropertyName = "is_optional")]
        public bool? IsOptional { get; set; }

        [JsonProperty(Order = 6, PropertyName = "is_updatable")]
        public bool? IsUpdatable { get; set; }

        [JsonProperty(Order = 7, PropertyName = "token_set")]
        public string TokenSet { get; set; }

        [JsonProperty(Order = 8, PropertyName = "default_value")]
        public string DefaultValue { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227", Justification = "Needs to be read/write for serialization.")]
        [JsonProperty(Order = 9, PropertyName = "possible_values")]
        public Dictionary<string, string> PossibleValues { get; set; }

        [JsonProperty(Order = 10, PropertyName = "validation")]
        public PropertyValidation Validation { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227", Justification = "Needs to be read/write for serialization.")]
        [JsonProperty(Order = 11, PropertyName = "transformation")]
        public Dictionary<string, PropertyTransformationInfo> Transformation { get; set; }

        [JsonProperty(Order = 12, PropertyName = "dataProtection")]
        public PropertyDataProtection DataProtection { get; set; }

        [JsonProperty(Order = 13, PropertyName = "indexedOn")]
        public string IndexedOn { get; set; }

        [JsonProperty(Order = 14, PropertyName = "display_property")]
        public string DisplayProperty { get; set; }

        [JsonProperty(Order = 15, PropertyName = "pidl_download_enabled")]
        public bool? PidlDownloadEnabled { get; set; }

        [JsonProperty(Order = 16, PropertyName = "pidl_download_parameter")]
        public string PidlDownloadParameter { get; set; }

        [JsonProperty(Order = 17, PropertyName = "displayOnly")]
        public bool? DisplayOnly { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227", Justification = "Needs to be read/write for serialization.")]
        [JsonProperty(Order = 18, PropertyName = "validations")]
        public List<PropertyValidation> Validations { get; set; }

        [JsonProperty(Order = 19, PropertyName = "useEdgeTokenization")]
        public bool? UseEdgeTokenization { get; set; }

        [JsonProperty(Order = 20, PropertyName = "keyPropertyResolutionMappings")]
        public List<List<string>> KeyPropertyResolutionMappings { get; set; }

        [JsonProperty(Order = 20, PropertyName = "onValidationFailed")]
        public Dictionary<string, bool> OnValidationFailed { get; set; }

        /// <summary>
        /// Gets or sets - Used by payment client to use conditional filed values
        /// </summary>
        [JsonProperty(Order = 21, PropertyName = "isConditionalFieldValue")]
        public bool? IsConditionalFieldValue { get; set; }

        /// <summary>
        /// Gets or sets - Used by payment client to receive broadcasted values
        /// </summary>
        [JsonProperty(Order = 22, PropertyName = "usePreExistingValue", NullValueHandling = NullValueHandling.Ignore)]
        public bool UsePreExistingValue { get; set; }

        /// <summary>
        /// Gets or sets - Used by payment client to broadcast values to other target filed
        /// </summary>
        [JsonProperty(Order = 23, PropertyName = "broadcastTo")]
        public string BroadcastTo { get; set; }

        [JsonProperty(Order = 24, PropertyName = "sideEffects")]
        [SuppressMessage("Microsoft.Usage", "CA2227", Justification = "Public setter used in serialization.")]
        public Dictionary<string, string> SideEffects { get; set; }

        // Allows model.ts in pidl.sdk to trigger eventHub.propertyUpdated
        // which triggers onEvent callback in pidl-react components
        [JsonProperty(Order = 25, PropertyName = "emitEventOnPropertyUpdate", NullValueHandling = NullValueHandling.Ignore)]
        public bool? EmitEventOnPropertyUpdate { get; set; }

        [JsonIgnore]
        public bool IsIdentityProperty { get; set; }
    }
}