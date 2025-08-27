// <copyright file="GoogleTokenTransformer.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.V7
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.Commerce.Payments.Common;
    using Microsoft.Commerce.Payments.Common.Tracing;
    using Microsoft.Commerce.Payments.PidlFactory.V7;
    using Microsoft.Commerce.Payments.PimsModel.V4;
    using Microsoft.Commerce.Payments.PXService.Model.PaymentOrchestratorService;
    using Microsoft.Commerce.Payments.PXService.Model.PXInternal;
    using Microsoft.Commerce.Tracing;
    using Newtonsoft.Json;

    public class GoogleTokenTransformer : IExternalPaymentTokenTransformer
    {
        private readonly GooglePayPayment googlePayPayment;
        private readonly EventTraceActivity traceActivityId;

        /// <summary>
        /// Initializes a new instance of the <see cref="GoogleTokenTransformer"/> class.
        /// </summary>
        public GoogleTokenTransformer()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GoogleTokenTransformer"/> class.
        /// </summary>
        /// <param name="googleToken">The Google Pay token containing payment data.</param>
        /// <param name="traceActivityId">Event Trace Activity Id for telemetry</param>
        public GoogleTokenTransformer(string googleToken, EventTraceActivity traceActivityId)
        {
            this.traceActivityId = traceActivityId;
            this.googlePayPayment = this.DeserializeGooglePayToken(googleToken);
        }

        /// <summary>
        /// transfrom language to language code
        /// </summary>
        /// <param name="language"> language string like en-US</param>
        /// <returns>Returns language code like en</returns>
        public static string TransformLanguage(string language)
        {
            string defaultLanguage = GlobalConstants.Defaults.Language.ToLower();
            if (string.IsNullOrWhiteSpace(language) || !language.Contains("-"))
            {
                return defaultLanguage;
            }

            return language.Split('-').FirstOrDefault() ?? defaultLanguage;
        }

        /// <summary>
        /// Extract AddressInfoV3 from google pay PIDL data
        /// </summary>
        /// <param name="addressType">Address type (billing/shipping)</param>
        /// <returns>Return addressinfov3</returns>
        public AddressInfoV3 ExtractAddressInfoV3(string addressType = V7.Constants.AddressTypes.Billing)
        {
            this.ValiateAddressType(addressType);

            if (string.Equals(addressType, V7.Constants.AddressTypes.Billing, StringComparison.OrdinalIgnoreCase))
            {
                return new AddressInfoV3
                {
                    AddressLine1 = this.googlePayPayment?.PaymentMethodData?.Info?.BillingAddress?.Address1,
                    AddressLine2 = this.googlePayPayment?.PaymentMethodData?.Info?.BillingAddress?.Address2,
                    AddressLine3 = this.googlePayPayment?.PaymentMethodData?.Info?.BillingAddress?.Address3,

                    City = this.googlePayPayment?.PaymentMethodData?.Info?.BillingAddress?.Locality,
                    Region = this.googlePayPayment?.PaymentMethodData?.Info?.BillingAddress?.AdministrativeArea,
                    PostalCode = this.googlePayPayment?.PaymentMethodData?.Info?.BillingAddress?.PostalCode,
                    Country = this.googlePayPayment?.PaymentMethodData?.Info?.BillingAddress?.CountryCode,
                    FirstName = this.googlePayPayment?.PaymentMethodData?.Info?.BillingAddress?.Name,
                    LastName = " ",
                    EmailAddress = this.googlePayPayment?.Email
                };
            }
            else
            {
                return new AddressInfoV3
                {
                    AddressLine1 = this.googlePayPayment?.ShippingAddress?.Address1,
                    AddressLine2 = this.googlePayPayment?.ShippingAddress?.Address2,
                    AddressLine3 = this.googlePayPayment?.ShippingAddress?.Address3,

                    City = this.googlePayPayment?.ShippingAddress?.Locality,
                    Region = this.googlePayPayment?.ShippingAddress?.AdministrativeArea,
                    PostalCode = this.googlePayPayment?.ShippingAddress?.PostalCode,
                    Country = this.googlePayPayment?.ShippingAddress?.CountryCode,
                    FirstName = this.googlePayPayment?.ShippingAddress?.Name,
                    LastName = " ",
                    EmailAddress = this.googlePayPayment?.Email
                };
            }
        }

        /// <summary>
        /// Extract address from google pay PIDL data
        /// </summary>
        /// <param name="addressType">Address type (billing/shipping)</param>
        /// <returns>Return addressinfov3</returns>
        public Model.PaymentOrchestratorService.Address ExtractAddress(string addressType = V7.Constants.AddressTypes.Billing)
        {
            this.ValiateAddressType(addressType);

            if (string.Equals(addressType, V7.Constants.AddressTypes.Billing, StringComparison.OrdinalIgnoreCase))
            {
                return new Model.PaymentOrchestratorService.Address
                {
                    AddressLine1 = this.googlePayPayment?.PaymentMethodData?.Info?.BillingAddress?.Address1,
                    AddressLine2 = this.googlePayPayment?.PaymentMethodData?.Info?.BillingAddress?.Address2,
                    AddressLine3 = this.googlePayPayment?.PaymentMethodData?.Info?.BillingAddress?.Address3,

                    City = this.googlePayPayment?.PaymentMethodData?.Info?.BillingAddress?.Locality,
                    Region = this.googlePayPayment?.PaymentMethodData?.Info?.BillingAddress?.AdministrativeArea,
                    PostalCode = this.googlePayPayment?.PaymentMethodData?.Info?.BillingAddress?.PostalCode,
                    Country = this.googlePayPayment?.PaymentMethodData?.Info?.BillingAddress?.CountryCode,
                    FirstName = this.googlePayPayment?.PaymentMethodData?.Info?.BillingAddress?.Name,
                    LastName = " ",
                };
            }
            else
            {
                return new Model.PaymentOrchestratorService.Address
                {
                    AddressLine1 = this.googlePayPayment?.ShippingAddress?.Address1,
                    AddressLine2 = this.googlePayPayment?.ShippingAddress?.Address2,
                    AddressLine3 = this.googlePayPayment?.ShippingAddress?.Address3,

                    City = this.googlePayPayment?.ShippingAddress?.Locality,
                    Region = this.googlePayPayment?.ShippingAddress?.AdministrativeArea,
                    PostalCode = this.googlePayPayment?.ShippingAddress?.PostalCode,
                    Country = this.googlePayPayment?.ShippingAddress?.CountryCode,
                    FirstName = this.googlePayPayment?.ShippingAddress?.Name,
                    LastName = " ",
                };
            }
        }

        public PIDLData ExtractPaymentInstrument(AttachmentType attachmentType)
        {
            var details = new Dictionary<string, object>
        {
            { V7.Constants.CreditCardPropertyDescriptionName.AccountHolderName, this.googlePayPayment?.PaymentMethodData.Info.BillingAddress?.Name },
            { V7.Constants.CreditCardPropertyDescriptionName.AccountToken, this.googlePayPayment?.PaymentMethodData?.Token },
            { V7.Constants.PropertyDescriptionIds.Address, new Dictionary<string, object>
                {
                    {
                        V7.Constants.PropertyDescriptionIds.AddressLine1, this.googlePayPayment?.PaymentMethodData?.Info?.BillingAddress?.Address1
                    },
                    {
                        V7.Constants.PropertyDescriptionIds.AddressLine2, this.googlePayPayment?.PaymentMethodData?.Info?.BillingAddress?.Address2
                    },
                    {
                        V7.Constants.PropertyDescriptionIds.AddressLine3, this.googlePayPayment?.PaymentMethodData?.Info?.BillingAddress?.Address3
                    },
                    {
                        V7.Constants.PropertyDescriptionIds.City, this.googlePayPayment?.PaymentMethodData?.Info?.BillingAddress?.Locality
                    },
                    {
                        V7.Constants.PropertyDescriptionIds.Region, this.googlePayPayment?.PaymentMethodData?.Info?.BillingAddress?.AdministrativeArea
                    },
                    {
                        V7.Constants.PropertyDescriptionIds.PostalCode, this.googlePayPayment?.PaymentMethodData?.Info?.BillingAddress?.PostalCode
                    },
                    {
                        V7.Constants.PropertyDescriptionIds.Country, this.googlePayPayment?.PaymentMethodData?.Info?.BillingAddress?.CountryCode
                    },
                }
            }
        };

            return new PIDLData
            {
                [V7.Constants.PropertyDescriptionIds.PaymentMethodFamily] = V7.Constants.PaymentMethodFamily.ewallet.ToString(),
                [V7.Constants.PropertyDescriptionIds.PaymentMethodType] = V7.Constants.PaymentMethodType.GooglePay,
                [V7.Constants.PropertyDescriptionIds.Details] = details,
                [V7.Constants.PropertyDescriptionIds.AttachmentType] = attachmentType
            };
        }

        /// <inheritdoc/>
        public string ExtractEmailAddress()
        {
            return this.googlePayPayment?.Email;
        }

        /// <inheritdoc/>
        /// TODO: delete this method after pr and cr are merged
        public Dictionary<string, object> GetButtonPayload(CheckoutRequestClientActions checkoutRequestClientActions, string partner, string actionType)
        {
            return new Dictionary<string, object>
            {
                { V7.Constants.ExpressCheckoutButtonPayloadKey.Amount, Convert.ToString(checkoutRequestClientActions?.Amount) },
                { V7.Constants.QueryParameterName.Country, checkoutRequestClientActions?.Country },
                { V7.Constants.QueryParameterName.Currency, checkoutRequestClientActions?.Currency },
                { V7.Constants.QueryParameterName.Partner, partner },
                { V7.Constants.QueryParameterName.Language, TransformLanguage(checkoutRequestClientActions?.Language) },
                { V7.Constants.ExpressCheckoutButtonPayloadKey.ActionType, actionType },
            };
        }

        public Dictionary<string, object> GetButtonPayload(PaymentRequestClientActions paymentRequestClientActions, string partner, string actionType)
        {
            return new Dictionary<string, object>
            {
                { V7.Constants.ExpressCheckoutButtonPayloadKey.Amount, Convert.ToString(paymentRequestClientActions?.Amount) },
                { V7.Constants.QueryParameterName.Country, paymentRequestClientActions?.Country },
                { V7.Constants.QueryParameterName.Currency, paymentRequestClientActions?.Currency },
                { V7.Constants.QueryParameterName.Partner, partner },
                { V7.Constants.QueryParameterName.Language, TransformLanguage(paymentRequestClientActions?.Language) },
                { V7.Constants.ExpressCheckoutButtonPayloadKey.ActionType, actionType },
            };
        }

        /// <inheritdoc/>
        public Dictionary<string, object> GetButtonPayload(ExpressCheckoutRequest expressCheckoutRequest, string partner, string actionType)
        {
            var buttonPayload = new Dictionary<string, object>
            {
                { V7.Constants.ExpressCheckoutButtonPayloadKey.Amount, Convert.ToString(expressCheckoutRequest?.Amount) },
                { V7.Constants.QueryParameterName.Country, expressCheckoutRequest?.Country },
                { V7.Constants.QueryParameterName.Currency, expressCheckoutRequest?.Currency },
                { V7.Constants.QueryParameterName.Partner, partner },
                { V7.Constants.QueryParameterName.Language, TransformLanguage(expressCheckoutRequest?.Language) },
                { V7.Constants.ExpressCheckoutButtonPayloadKey.ActionType, actionType },
            };

            if (expressCheckoutRequest?.RecurringPaymentDetails != null)
            {
                buttonPayload.Add(V7.Constants.ExpressCheckoutButtonPayloadKey.RecurringPaymentDetails, expressCheckoutRequest.RecurringPaymentDetails);
            }

            if (expressCheckoutRequest?.Options != null)
            {
                buttonPayload.Add(V7.Constants.ExpressCheckoutButtonPayloadKey.Options, expressCheckoutRequest.Options);
            }

            return buttonPayload;
        }

        private GooglePayPayment DeserializeGooglePayToken(string googlePayToken)
        {
            if (googlePayToken == null)
            {
                throw TraceCore.TraceException(this.traceActivityId, new NotSupportedException($"Google pay token should not be null"));
            }

            GooglePayPayment googlePayData = null;

            try
            {
                googlePayData = JsonConvert.DeserializeObject<GooglePayPayment>(googlePayToken);
            }
            catch
            {
                throw TraceCore.TraceException(this.traceActivityId, new FailedOperationException($"Failed to deserialize google pay data. Token: {googlePayToken}"));
            }

            return googlePayData;
        }

        private void ValiateAddressType(string addressType)
        {
            if (string.IsNullOrWhiteSpace(addressType)
                || (!string.Equals(addressType, V7.Constants.AddressTypes.Shipping, StringComparison.OrdinalIgnoreCase)
                    && !string.Equals(addressType, V7.Constants.AddressTypes.Billing, StringComparison.OrdinalIgnoreCase)))
            {
                throw TraceCore.TraceException(this.traceActivityId, new NotSupportedException("Invalid address type. Must be 'billing' or 'shipping'." + nameof(addressType)));
            }
        }
    }
}