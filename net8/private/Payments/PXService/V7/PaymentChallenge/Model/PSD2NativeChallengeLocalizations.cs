// <copyright file="PSD2NativeChallengeLocalizations.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.V7.PaymentChallenge.Model
{
    using Newtonsoft.Json;

    public class PSD2NativeChallengeLocalizations
    {
        [JsonProperty(PropertyName = "challengePageHeader")]
        public string ChallengePageHeader { get; set; }

        [JsonProperty(PropertyName = "cancelButtonLabel")]
        public string CancelButtonLabel { get; set; }

        [JsonProperty(PropertyName = "backButtonLabel")]
        public string BackButtonLabel { get; set; }

        [JsonProperty(PropertyName = "backButtonAccessibilityLabel")]
        public string BackButtonAccessibilityLabel { get; set; }

        [JsonProperty(PropertyName = "cancelButtonAccessibilityLabel")]
        public string CancelButtonAccessibilityLabel { get; set; }

        [JsonProperty(PropertyName = "orderAccessibilityLabel")]
        public string OrderingAccessibilityLabel { get; set; }

        [JsonProperty(PropertyName = "bankLogoAccessibilityLabel")]
        public string BankLogoAccessibilityLabel { get; set; }

        [JsonProperty(PropertyName = "cardLogoAccessibilityLabel")]
        public string CardLogoAccessibilityLabel { get; set; }
    }
}