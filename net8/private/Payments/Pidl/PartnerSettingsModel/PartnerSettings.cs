// <copyright file="PartnerSettings.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PartnerSettingsModel
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using Newtonsoft.Json;

    public class PartnerSettings
    {
        public PartnerSettings()
        {
        }

        public PartnerSettings(Dictionary<string, PaymentExperienceSetting> paymentExperienceSettings)
        {
            this.PaymentExperienceSettings = new Dictionary<string, PaymentExperienceSetting>(paymentExperienceSettings);
        }

        [JsonProperty("paymentExperienceSettings")]
        [SuppressMessage("Microsoft.Usage", "CA2227", Justification = "Needed to be set.")]
        public Dictionary<string, PaymentExperienceSetting> PaymentExperienceSettings { get; set; }
    }
}
