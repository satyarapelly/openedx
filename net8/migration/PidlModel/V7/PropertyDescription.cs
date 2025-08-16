// <copyright file="PropertyDescription.cs" company="Microsoft">Copyright (c) Microsoft 2015. All rights reserved.</copyright>
namespace Microsoft.Commerce.Payments.PidlModel.V7
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using Newtonsoft.Json;

    public class PropertyDescription
    {
        private string defaultValue;
        private Dictionary<string, string> possibleValues;
        private Dictionary<string, string> localizedPossibleValues;
        private List<List<string>> keyPropertyResolutionMappings;
        private Dictionary<string, PropertyTransformationInfo> transformations;
        private List<PropertyValidation> validations;
        private bool skipPossibleValuesLocalization = false;
        private Dictionary<string, string> sideEffects;
        private Dictionary<string, bool> onValidationFailed;

        public PropertyDescription()
        {
        }

        public PropertyDescription(
            string propertyDescriptionId,
            string propertyType,
            string dataType,
            string displayProperty,
            bool isKey,
            bool isOptional,
            bool isUpdatable,
            string tokenSet,
            string defaultValue,
            Dictionary<string, string> possibleValues,
            bool? pidlDownloadEnabled,
            string pidlDownloadParameter,
            bool? displayOnly)
        {
            this.PropertyDescriptionId = propertyDescriptionId;
            this.PropertyType = propertyType;
            this.DataType = dataType;
            this.PropertyDescriptionType = dataType;
            this.DisplayProperty = displayProperty;
            this.IsKey = isKey;
            this.IsOptional = isOptional;
            this.IsUpdatable = isUpdatable;
            this.TokenSet = tokenSet;
            this.defaultValue = defaultValue;
            this.possibleValues = possibleValues;
            this.PidlDownloadEnabled = pidlDownloadEnabled;
            this.PidlDownloadParameter = pidlDownloadParameter;
            this.DisplayOnly = displayOnly;
        }

        public PropertyDescription(PropertyDescription template, Dictionary<string, string> contextTable, bool skipPossibleValuesLocalization)
        {
            this.PropertyDescriptionId = template.PropertyDescriptionId;
            this.PropertyType = template.PropertyType;
            this.DataType = template.DataType;
            this.PropertyDescriptionType = template.PropertyDescriptionType;
            this.DisplayProperty = template.DisplayProperty;
            this.IsId = template.IsId;
            this.IsKey = template.IsKey;
            this.IsOptional = template.IsOptional;
            this.IsUpdatable = template.IsUpdatable;
            this.TokenSet = template.TokenSet;
            this.defaultValue = template.defaultValue;
            this.PidlDownloadEnabled = template.PidlDownloadEnabled;
            this.PidlDownloadParameter = template.PidlDownloadParameter;
            this.DisplayOnly = template.DisplayOnly;
            if (template.PossibleValues != null)
            {
                this.possibleValues = new Dictionary<string, string>(template.possibleValues);
            }

            this.SetValidationList(template.Validations, contextTable);
            this.transformations = template.transformations == null ? null : new Dictionary<string, PropertyTransformationInfo>(template.transformations);

            foreach (KeyValuePair<string, string> contextKeyValue in contextTable)
            {
                this.defaultValue = this.defaultValue == null ? null : this.defaultValue.Replace(contextKeyValue.Key, contextKeyValue.Value);
            }

            this.skipPossibleValuesLocalization = skipPossibleValuesLocalization;

            if (template.KeyPropertyResolutionMappings != null)
            {
                this.keyPropertyResolutionMappings = new List<List<string>>(template.KeyPropertyResolutionMappings);
            }
        }

        [JsonIgnore]
        public string PropertyDescriptionId { get; private set; }

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
        public object DefaultValue
        {
            get
            {
                if (this.defaultValue == null)
                {
                    return null;
                }

                if (string.Equals(this.DataType, "bool", System.StringComparison.OrdinalIgnoreCase))
                {
                    bool boolTypeDefaultValue;
                    if (bool.TryParse(this.defaultValue, out boolTypeDefaultValue))
                    {
                        return boolTypeDefaultValue;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return this.defaultValue == null ? null : PidlModelHelper.GetNonParameterizedString(this.defaultValue);
                }
            }

            set
            {
                this.defaultValue = value == null ? null : value.ToString();
            }
        }

        [JsonProperty(Order = 9, PropertyName = "possible_values")]
        [SuppressMessage("Microsoft.Usage", "CA2227", Justification = "Public setter used in serialization.")]
        public Dictionary<string, string> PossibleValues
        {
            get
            {
                if (this.localizedPossibleValues != null)
                {
                    return this.localizedPossibleValues;
                }

                if (null != this.possibleValues)
                {
                    this.UpdateLocalizedPossibleValues();
                }

                return this.localizedPossibleValues;
            }

            set
            {
                this.possibleValues = value;
            }
        }

        [JsonProperty(Order = 10, PropertyName = "validation")]
        public PropertyValidation Validation
        {
            get
            {
                return this.validations?.FirstOrDefault();
            }

            set
            {
                if (value != null)
                {
                    this.validations = new List<PropertyValidation>();
                    this.validations.Add(value);
                }
            }
        }

        [JsonProperty(Order = 11, PropertyName = "transformation")]
        [SuppressMessage("Microsoft.Usage", "CA2227", Justification = "Public setter used in serialization.")]
        public Dictionary<string, PropertyTransformationInfo> Transformation
        {
            get
            {
                return this.transformations;
            }

            set
            {
                this.transformations = value;
            }
        }

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

        [JsonProperty(Order = 18, PropertyName = "validations")]
        [SuppressMessage("Microsoft.Usage", "CA2227", Justification = "Public setter used in serialization.")]
        public List<PropertyValidation> Validations
        {
            get
            {
                return this.validations;
            }

            set
            {
                this.validations = value;
            }
        }

        [JsonProperty(Order = 19, PropertyName = "useEdgeTokenization")]
        public bool? UseEdgeTokenization { get; set; }

        [JsonProperty(Order = 20, PropertyName = "keyPropertyResolutionMappings")]
        [SuppressMessage("Microsoft.Usage", "CA2227", Justification = "Public setter used in serialization.")]
        public List<List<string>> KeyPropertyResolutionMappings
        {
            get
            {
                return this.keyPropertyResolutionMappings;
            }

            set
            {
                this.keyPropertyResolutionMappings = value;
            }
        }

        [JsonProperty(Order = 21, PropertyName = "isConditionalFieldValue")]
        public bool? IsConditionalFieldValue { get; set; }

        [JsonProperty(Order = 22, PropertyName = "broadcastTo")]
        public string BroadcastTo { get; set; }

        [JsonProperty(Order = 23, PropertyName = "sideEffects")]
        [SuppressMessage("Microsoft.Usage", "CA2227", Justification = "Public setter used in serialization.")]
        public Dictionary<string, string> SideEffects
        {
            get
            {
                return this.sideEffects;
            }

            set
            {
                this.sideEffects = value == null ? null : new Dictionary<string, string>(value);
            }
        }

        [JsonProperty(Order = 23, PropertyName = "onValidationFailed")]
        [SuppressMessage("Microsoft.Usage", "CA2227", Justification = "Public setter used in serialization.")]
        public Dictionary<string, bool> OnValidationFailed
        {
            get
            {
                return this.onValidationFailed;
            }

            set
            {
                this.onValidationFailed = value == null ? null : new Dictionary<string, bool>(value);
            }
        }

        [JsonProperty(Order = 24, PropertyName = "usePreExistingValue", NullValueHandling = NullValueHandling.Ignore)]
        public bool? UsePreExistingValue { get; set; }

        // Allows model.ts in pidl.sdk to trigger eventHub.propertyUpdated
        // which triggers onEvent callback in pidl-react components
        [JsonProperty(Order = 25, PropertyName = "emitEventOnPropertyUpdate", NullValueHandling = NullValueHandling.Ignore)]
        public bool? EmitEventOnPropertyUpdate { get; set; }

        [JsonIgnore]
        public bool IsIdentityProperty { get; set; }

        public void AddPossibleValue(string key, string value)
        {
            if (this.possibleValues == null)
            {
                this.possibleValues = new Dictionary<string, string>();
            }

            this.possibleValues[key] = value;
            this.UpdateLocalizedPossibleValues();
        }

        public void AddTransformation(Dictionary<string, PropertyTransformationInfo> transformation)
        {
            if (this.transformations == null)
            {
                this.transformations = new Dictionary<string, PropertyTransformationInfo>();
            }

            this.transformations = transformation;
        }

        public void UpdatePossibleValues(Dictionary<string, string> value)
        {
            if (value != null)
            {
                this.possibleValues = new Dictionary<string, string>(value);
                this.UpdateLocalizedPossibleValues();
            }
        }

        public void RemovePossibleValues()
        {
            this.possibleValues = null;
            this.localizedPossibleValues = null;
        }

        public void SetValidationList(List<PropertyValidation> value, Dictionary<string, string> contextTable)
        {
            if (value != null && value.Count > 0)
            {
                this.validations = new List<PropertyValidation>();
                foreach (PropertyValidation validation in value)
                {
                    this.validations.Add(new PropertyValidation(validation, contextTable));
                }
            }
        }

        public void AddAdditionalValidation(PropertyValidation value)
        {
            if (value != null)
            {
                if (this.validations == null)
                {
                    this.validations = new List<PropertyValidation>();
                }

                this.validations.Add(value);
            }
        }

        public void SetSkipPossibleValuesLocalization(bool value)
        {
            this.skipPossibleValuesLocalization = value;
        }

        public void UpdateKeyPropertyResolutionMappings(List<List<string>> value)
        {
            if (value != null)
            {
                this.keyPropertyResolutionMappings = new List<List<string>>(value);
            }
        }

        private void UpdateLocalizedPossibleValues()
        {
            this.localizedPossibleValues = new Dictionary<string, string>(this.possibleValues.Count);
            foreach (KeyValuePair<string, string> keyValuePair in this.possibleValues)
            {
                this.localizedPossibleValues[PidlModelHelper.GetNonParameterizedString(keyValuePair.Key)] = keyValuePair.Value == null ? keyValuePair.Key : (this.skipPossibleValuesLocalization ? keyValuePair.Value : PidlModelHelper.GetLocalizedString(keyValuePair.Value));
            }
        }
    }
}