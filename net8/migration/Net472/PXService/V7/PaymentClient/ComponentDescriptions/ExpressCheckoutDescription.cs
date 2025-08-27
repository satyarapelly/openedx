// <copyright file="ExpressCheckoutDescription.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.Commerce.Payments.Common;
    using Microsoft.Commerce.Payments.Common.Tracing;
    using Microsoft.Commerce.Payments.PidlFactory.V7;
    using Microsoft.Commerce.Payments.PidlModel.V7;
    using Microsoft.Commerce.Payments.PimsModel.V4;
    using Microsoft.Commerce.Payments.PXCommon;
    using Microsoft.Commerce.Payments.PXService.Model.PXInternal;
    using Microsoft.Commerce.Payments.PXService.V7;
    using Newtonsoft.Json;

    public class ExpressCheckoutDescription : ComponentDescription
    {
        private string descriptionType = V7.Constants.DescriptionTypes.Checkout;
        private string descriptionId = V7.Constants.PIDLResourceDescriptionId.ExpressCheckout;
        private ExpressCheckoutRequest expressCheckoutRequest = new ExpressCheckoutRequest();

        public ExpressCheckoutDescription(string expressCheckoutRequest, IList<PaymentMethod> paymentMethods)
        {
            this.expressCheckoutRequest = this.DeserializeExpressCheckoutRequestData(expressCheckoutRequest);
            this.PaymentMethods = paymentMethods;
        }

        public ExpressCheckoutRequest ExpressCheckoutRequest
        {
            get { return this.expressCheckoutRequest; }
        }

        public override string DescriptionType
        {
            get
            {
                return this.descriptionType;
            }
        }

        public override async Task<List<PIDLResource>> GetDescription()
        {
            var paymentMethods = QuickPaymentDescription.GetQuickPaymentMethods(this.PaymentMethods);
            ComponentDescription.ValidatePaymentMethods(paymentMethods, V7.Constants.Component.ExpressCheckout, this.TraceActivityId);

            DataSource dataSource = new DataSource();
            if (this.ExposedFlightFeatures?.Contains(PXCommon.Flighting.Features.PXUseMockWalletConfig, StringComparer.OrdinalIgnoreCase) ?? false)
            {
                // Using static wallet config - This will be removed and get config from wallet service
                string walletConfig = "{\"dataSourceConfig\":{\"useLocalDataSource\":true},\"members\":[{\"PIDLConfig\":{\"SelectResource.PaymentInstrument\":{\"actions\":{\"ewallet.applepay.default\":[\"PaymentInstrumentHandler.ClientSupported\"],\"ewallet.googlepay.default\":[\"PaymentInstrumentHandler.ClientSupported\"]}},\"HandlePaymentChallenge\":{\"actions\":{\"ewallet.applepay.default\":[\"PaymentInstrumentHandler.CollectPaymentToken\"],\"ewallet.googlepay.default\":[\"PaymentInstrumentHandler.CollectPaymentToken\"]}}},\"PaymentInstrumentHandlers\":[{\"allowedAuthMethods\":[\"PAN_ONLY\"],\"protocolVersion\":\"ECv2\",\"publicKey\":\"BCmwmb/YJfBKlT/wyGN+50EY03RlOXlCAfq1MAgIpSHC7lrNW6i1gQnVCC4y5WuGurgQpf4fzQtPEly1TkC1usY=\",\"merchantName\":\"Microsoft\",\"apiMajorVersion\":\"2\",\"apiMinorVersion\":\"0\",\"assuranceDetailsRequired\":true,\"publicKeyVersion\":\"01032025\",\"enableGPayIframeForAllBrowsers\":false,\"merchantId\":\"BCR2DN4TZ244PH2A\",\"paymentMethodFamily\":\"ewallet\",\"paymentMethodType\":\"googlepay\",\"payLabel\":\"amount due plus applicable taxes\",\"enableBillingAddress\":true,\"enableEmail\":true,\"piid\":\"cdc85313-9b57-4052-81fb-dea336132cbf\",\"integrationType\":\"DIRECT\",\"allowedAuthMethodsPerCountry\":{\"us\":[\"VISA\",\"MASTERCARD\",\"DISCOVER\",\"AMEX\"],\"ca\":[\"VISA\",\"MASTERCARD\",\"DISCOVER\",\"AMEX\"],\"fr\":[\"VISA\",\"MASTERCARD\",\"AMEX\"],\"de\":[\"VISA\",\"MASTERCARD\",\"AMEX\"],\"it\":[\"VISA\",\"MASTERCARD\",\"AMEX\"],\"es\":[\"VISA\",\"MASTERCARD\",\"AMEX\"],\"gb\":[\"VISA\",\"MASTERCARD\",\"AMEX\"]},\"clientSupported\":{\"supportedBrowsers\":{\"chrome\":\"100\",\"edge\":\"100\"},\"supportedOS\":{\"ios\":\"16.0\",\"android\":\"16.0\",\"windows\":\"16.0\"},\"additionalAPIsCheck\":[\"canMakePayment\"]}},{\"merchantCapabilities\":[\"supports3DS\",\"supportsEMV\"],\"displayName\":\"Microsoft\",\"initiative\":\"Web\",\"initiativeContext\":\"mystore.example.com\",\"merchantIdentifier\":\"merchant.com.microsoft.paymicrosoft.sandbox\",\"applePayVersion\":\"3\",\"paymentMethodFamily\":\"ewallet\",\"paymentMethodType\":\"applepay\",\"payLabel\":\"amount due plus applicable taxes\",\"enableBillingAddress\":true,\"enableEmail\":true,\"piid\":\"be4de87d-7e38-4b2d-8836-9237eb32848e\",\"integrationType\":\"DIRECT\",\"allowedAuthMethodsPerCountry\":{\"us\":[\"VISA\",\"MASTERCARD\",\"DISCOVER\",\"AMEX\"],\"ca\":[\"VISA\",\"MASTERCARD\",\"DISCOVER\",\"AMEX\"],\"fr\":[\"VISA\",\"MASTERCARD\",\"AMEX\"],\"de\":[\"VISA\",\"MASTERCARD\",\"AMEX\"],\"it\":[\"VISA\",\"MASTERCARD\",\"AMEX\"],\"es\":[\"VISA\",\"MASTERCARD\",\"AMEX\"],\"gb\":[\"VISA\",\"MASTERCARD\",\"AMEX\"]},\"clientSupported\":{\"supportedBrowsers\":{\"safari\":\"16.1\"},\"supportedOS\":{\"ios\":\"15.0\"},\"additionalAPIsCheck\":[\"canMakePaymentWithActiveCard\"],\"paymentProxyRequired\":{\"safari\":\"16.5\"}}}]}]}";

                if (Common.Environments.Environment.IsProdOrPPEEnvironment)
                {
                    walletConfig = "{\"dataSourceConfig\":{\"useLocalDataSource\":true},\"members\":[{\"PIDLConfig\":{\"SelectResource.PaymentInstrument\":{\"actions\":{\"ewallet.applepay.default\":[\"PaymentInstrumentHandler.ClientSupported\"],\"ewallet.googlepay.default\":[\"PaymentInstrumentHandler.ClientSupported\"]}},\"HandlePaymentChallenge\":{\"actions\":{\"ewallet.applepay.default\":[\"PaymentInstrumentHandler.CollectPaymentToken\"],\"ewallet.googlepay.default\":[\"PaymentInstrumentHandler.CollectPaymentToken\"]}}},\"PaymentInstrumentHandlers\":[{\"allowedAuthMethods\":[\"PAN_ONLY\"],\"protocolVersion\":\"ECv2\",\"publicKey\":\"BBMH2x40787Ayspaqg2My43ZkLb4QvHPuDfE/VhQeGKzHy6JIcKPMyHifOe9Rav4bHWJG4W+aJpi0eKQbRlWY5M=\",\"merchantName\":\"Microsoft\",\"apiMajorVersion\":\"2\",\"apiMinorVersion\":\"0\",\"assuranceDetailsRequired\":true,\"publicKeyVersion\":\"01172025\",\"enableGPayIframeForAllBrowsers\":false,\"merchantId\":\"BCR2DN4TZ244PH2A\",\"paymentMethodFamily\":\"ewallet\",\"paymentMethodType\":\"googlepay\",\"payLabel\":\"amount due plus applicable taxes\",\"enableBillingAddress\":true,\"enableEmail\":true,\"piid\":\"cdc85313-9b57-4052-81fb-dea336132cbf\",\"integrationType\":\"DIRECT\",\"allowedAuthMethodsPerCountry\":{\"us\":[\"VISA\",\"MASTERCARD\",\"DISCOVER\",\"AMEX\"],\"ca\":[\"VISA\",\"MASTERCARD\",\"DISCOVER\",\"AMEX\"],\"fr\":[\"VISA\",\"MASTERCARD\",\"AMEX\"],\"de\":[\"VISA\",\"MASTERCARD\",\"AMEX\"],\"it\":[\"VISA\",\"MASTERCARD\",\"AMEX\"],\"es\":[\"VISA\",\"MASTERCARD\",\"AMEX\"],\"gb\":[\"VISA\",\"MASTERCARD\",\"AMEX\"]},\"clientSupported\":{\"supportedBrowsers\":{\"chrome\":\"100\",\"edge\":\"100\"},\"supportedOS\":{\"ios\":\"16.0\",\"android\":\"16.0\",\"windows\":\"16.0\"},\"additionalAPIsCheck\":[\"canMakePayment\"]}},{\"merchantCapabilities\":[\"supports3DS\",\"supportsEMV\"],\"displayName\":\"Microsoft\",\"initiative\":\"Web\",\"initiativeContext\":\"mystore.example.com\",\"merchantIdentifier\":\"merchant.com.microsoft.paymicrosoft.prod\",\"applePayVersion\":\"3\",\"paymentMethodFamily\":\"ewallet\",\"paymentMethodType\":\"applepay\",\"payLabel\":\"amount due plus applicable taxes\",\"enableBillingAddress\":true,\"enableEmail\":true,\"piid\":\"be4de87d-7e38-4b2d-8836-9237eb32848e\",\"integrationType\":\"DIRECT\",\"allowedAuthMethodsPerCountry\":{\"us\":[\"VISA\",\"MASTERCARD\",\"DISCOVER\",\"AMEX\"],\"ca\":[\"VISA\",\"MASTERCARD\",\"DISCOVER\",\"AMEX\"],\"fr\":[\"VISA\",\"MASTERCARD\",\"AMEX\"],\"de\":[\"VISA\",\"MASTERCARD\",\"AMEX\"],\"it\":[\"VISA\",\"MASTERCARD\",\"AMEX\"],\"es\":[\"VISA\",\"MASTERCARD\",\"AMEX\"],\"gb\":[\"VISA\",\"MASTERCARD\",\"AMEX\"]},\"clientSupported\":{\"supportedBrowsers\":{\"safari\":\"16.1\"},\"supportedOS\":{\"ios\":\"15.0\"},\"additionalAPIsCheck\":[\"canMakePaymentWithActiveCard\"],\"paymentProxyRequired\":{\"safari\":\"16.5\"}}}]}]}";
                }

                dataSource = JsonConvert.DeserializeObject<DataSource>(walletConfig);
            }
            else
            {
                // Get data sourece for given payment methods
                dataSource = await this.GetWalletConfigDataSource(paymentMethods, this.ExposedFlightFeatures?.ToList());
            }

            if (this.expressCheckoutRequest.IsTaxIncluded)
            {
                // Update payLabel to indicate Tax is inlcuded in the amount
                ComponentDescription.UpdatePayLabel(dataSource, this.expressCheckoutRequest?.Language);
            }

            // PIDL Generation
            List<PIDLResource> retVal = PIDLResourceFactory.Instance.GetPaymentClientDescriptions(this.descriptionType, this.Country, this.descriptionId, this.Operation, this.Language, this.Partner, exposedFlightFeatures: this.ExposedFlightFeatures?.ToList(), scenario: this.Scenario, setting: this.PSSSetting);

            retVal.ForEach(pidl =>
            {
                // Adding data source to PIDL
                pidl.DataSources = new Dictionary<string, DataSource>()
                {
                    { V7.Constants.WalletConfigConstants.WalletConfig, dataSource }
                };

                // Updates payload to google pay button
                UpdateExpressCheckoutButton(pidl, paymentMethods, V7.Constants.DisplayHintIds.GooglepayExpressCheckoutFrame, V7.Constants.PaymentMethodType.GooglePay);

                // Updates payload to apple pay button
                UpdateExpressCheckoutButton(pidl, paymentMethods, V7.Constants.DisplayHintIds.ApplepayExpressCheckoutFrame, V7.Constants.PaymentMethodType.ApplePay);

                // Update submit urls for express checkout.
                ComponentDescription.UpdateSubmitURL(pidl, V7.Constants.ButtonDisplayHintIds.SubmitButtonHidden, GlobalConstants.HTTPVerbs.POST, string.Format(V7.Constants.SubmitUrls.ExpressCheckoutConfirm, this.Partner, this.Country, this.Language));
            });

            return retVal;
        }

        private void UpdateExpressCheckoutButton(PIDLResource pidl, IList<PaymentMethod> paymentMethods, string displayHintId, string paymentMethodType)
        {
            var expressCheckoutButton = pidl.GetDisplayHintById(displayHintId) as ExpressCheckoutButtonDisplayHint;

            this.UpdateExpressCheckoutButtonSourceUrl(displayHintId, pidl);

            if (expressCheckoutButton != null)
            {
                if (paymentMethods.Any(pm => pm.PaymentMethodType.Equals(paymentMethodType, StringComparison.OrdinalIgnoreCase)))
                {
                    expressCheckoutButton.Payload = ExternalPaymentTokenTransformerFactory.Instance(paymentMethodType, this.TraceActivityId).GetButtonPayload(this.expressCheckoutRequest, this.Partner, DisplayHintActionType.triggerSubmit.ToString());
                }
                else
                {
                    pidl.RemoveDisplayHintById(displayHintId);
                }
            }
        }

        private ExpressCheckoutRequest DeserializeExpressCheckoutRequestData(string requestData)
        {
            if (requestData == null)
            {
                throw TraceCore.TraceException(this.TraceActivityId, new NotSupportedException($"Express checkout request data should not be null"));
            }

            ExpressCheckoutRequest expressCheckoutRequestData = null;

            try
            {
                expressCheckoutRequestData = JsonConvert.DeserializeObject<ExpressCheckoutRequest>(requestData);
            }
            catch
            {
                throw TraceCore.TraceException(this.TraceActivityId, new FailedOperationException($"Failed to deserialize express checkout reqeust data: {requestData}"));
            }

            return expressCheckoutRequestData;
        }
    }
}