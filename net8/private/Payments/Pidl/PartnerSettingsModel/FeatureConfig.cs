// <copyright file="FeatureConfig.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PartnerSettingsModel
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Reflection;
    using Newtonsoft.Json;

    public class FeatureConfig
    {
        [JsonProperty(PropertyName = "applicableMarkets")]
        [SuppressMessage("Microsoft.Usage", "CA2227", Justification = "Needed to be set.")]
        public List<string> ApplicableMarkets { get; set; }

        [JsonProperty(PropertyName = "displayCustomizationDetail")]
        [SuppressMessage("Microsoft.Usage", "CA2227", Justification = "Needed to be set.")]
        public List<DisplayCustomizationDetail> DisplayCustomizationDetail { get; set; }

        public bool DisplayCustomizationDetailEnabled(string wantedDetail)
        {
            if (this.DisplayCustomizationDetail != null && this.DisplayCustomizationDetail.Count > 0)
            {
                foreach (DisplayCustomizationDetail displayHintCustomizationDetail in this.DisplayCustomizationDetail)
                {
                    PropertyInfo property = displayHintCustomizationDetail.GetType().GetProperty(wantedDetail);

                    var detailValue = property.GetValue(displayHintCustomizationDetail);
                    if (property != null && detailValue != null)
                    {
                        return (bool)detailValue;
                    }
                }
            }

            return false;
        }

        public bool DisplayCustomizationDetailEnabled(string wantedDetail, object expectedValue)
        {
            if (this.DisplayCustomizationDetail != null && this.DisplayCustomizationDetail.Count > 0)
            {
                foreach (DisplayCustomizationDetail displayHintCustomizationDetail in this.DisplayCustomizationDetail)
                {
                    PropertyInfo property = displayHintCustomizationDetail.GetType().GetProperty(wantedDetail);

                    if (property != null)
                    {
                        var detailValue = property.GetValue(displayHintCustomizationDetail);
                        
                        if (detailValue != null && expectedValue != null && detailValue.Equals(expectedValue))
                        {
                           return true;
                        }
                    }
                }
            }

            return false;
        }

        public bool IsDisplayCustomizationDetailEnabledForFeature(FeatureConfig featureConfig, Dictionary<string, object> displayCustomizationDetails)
        {
            foreach (var customizationDetails in displayCustomizationDetails)
            {
                if (!this.DisplayCustomizationDetailEnabled(customizationDetails.Key, customizationDetails.Value))
                {
                    return false;
                }
            }

            return true;
        }
    }
}