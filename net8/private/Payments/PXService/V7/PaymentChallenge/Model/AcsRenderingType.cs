// <copyright file="AcsRenderingType.cs" company="Microsoft">Copyright (c) Microsoft 2020. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.V7.PaymentChallenge.Model
{
    using Newtonsoft.Json;
    using PayerAuth = Microsoft.Commerce.Payments.PXService.Model.PayerAuthService;

    public class AcsRenderingType
    {
        public AcsRenderingType(PayerAuth.AcsRenderingType acsRenderingType)
        {
            this.AcsInterface = acsRenderingType?.AcsInterface;
            this.AcsUiTemplate = acsRenderingType?.AcsUiTemplate;
        }

        [JsonProperty(PropertyName = "acsInterface")]
        public string AcsInterface { get; set; }

        [JsonProperty(PropertyName = "acsUiTemplate")]
        public string AcsUiTemplate { get; set; }
    }
}