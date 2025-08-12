// <copyright file="AppleTokenTransformer.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

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
    using Newtonsoft.Json;

    public class AppleTokenTransformer : IExternalPaymentTokenTransformer
    {
        private readonly ApplePayPayment applePayPayment;
        private readonly EventTraceActivity traceActivityId;

        /// <summary>
        /// Initializes a new instance of the <see cref="AppleTokenTransformer"/> class.
        /// </summary>
        public AppleTokenTransformer()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AppleTokenTransformer"/> class.
        /// </summary>
        /// <param name="appleToken">The Apple Pay token containing payment data.</param>
        /// <param name="traceActivityId">Event Trace Activity Id for telemetry</param>
        public AppleTokenTransformer(string appleToken, EventTraceActivity traceActivityId)
        {
            this.traceActivityId = traceActivityId;
            this.applePayPayment = this.DeserializeApplePayToken(appleToken);
        }

        /// <summary>
        /// Extract AddressInfoV3 from apple pay PIDL data
        /// </summary>
        /// <param name="addressType">Address type (billing/shipping)</param>
        /// <returns>Return addressinfov3</returns>
        public AddressInfoV3 ExtractAddressInfoV3(string addressType = V7.Constants.AddressTypes.Billing)
        {
            this.ValiateAddressType(addressType);

            string emailAddress = this.applePayPayment?.ShippingContact?.EmailAddress;

            if (string.Equals(addressType, V7.Constants.AddressTypes.Billing, StringComparison.OrdinalIgnoreCase))
            {
                return new AddressInfoV3
                {
                    AddressLine1 = this.applePayPayment?.BillingContact?.AddressLines?.FirstOrDefault(),
                    AddressLine2 = this.applePayPayment?.BillingContact?.AddressLines?.Skip(1).FirstOrDefault(),
                    AddressLine3 = this.applePayPayment?.BillingContact?.AddressLines?.Skip(2).FirstOrDefault(),

                    City = this.applePayPayment?.BillingContact?.Locality,
                    Region = this.applePayPayment?.BillingContact?.AdministrativeArea,
                    PostalCode = this.applePayPayment?.BillingContact?.PostalCode,
                    Country = this.applePayPayment?.BillingContact?.CountryCode,
                    FirstName = this.applePayPayment?.BillingContact?.GivenName,
                    LastName = this.applePayPayment?.BillingContact?.FamilyName,
                    EmailAddress = emailAddress
                };
            }
            else
            {
                return new AddressInfoV3
                {
                    AddressLine1 = this.applePayPayment?.ShippingContact?.AddressLines?.FirstOrDefault(),
                    AddressLine2 = this.applePayPayment?.ShippingContact?.AddressLines?.Skip(1).FirstOrDefault(),
                    AddressLine3 = this.applePayPayment?.ShippingContact?.AddressLines?.Skip(2).FirstOrDefault(),

                    City = this.applePayPayment?.ShippingContact?.Locality,
                    Region = this.applePayPayment?.ShippingContact?.AdministrativeArea,
                    PostalCode = this.applePayPayment?.ShippingContact?.PostalCode,
                    Country = this.applePayPayment?.ShippingContact?.Country,
                    FirstName = this.applePayPayment?.ShippingContact?.GivenName,
                    LastName = this.applePayPayment?.ShippingContact?.FamilyName,
                    EmailAddress = emailAddress
                };
            }
        }

        /// <summary>
        /// Extract Address from apple pay PIDL data
        /// </summary>
        /// <param name="addressType">Address type (billing/shipping)</param>
        /// <returns>Return address</returns>
        public Model.PaymentOrchestratorService.Address ExtractAddress(string addressType = V7.Constants.AddressTypes.Billing)
        {
            this.ValiateAddressType(addressType);

            if (string.Equals(addressType, V7.Constants.AddressTypes.Billing, StringComparison.OrdinalIgnoreCase))
            {
                return new Model.PaymentOrchestratorService.Address
                {
                    AddressLine1 = this.applePayPayment?.BillingContact?.AddressLines?.FirstOrDefault(),
                    AddressLine2 = this.applePayPayment?.BillingContact?.AddressLines?.Skip(1).FirstOrDefault(),
                    AddressLine3 = this.applePayPayment?.BillingContact?.AddressLines?.Skip(2).FirstOrDefault(),

                    City = this.applePayPayment?.BillingContact?.Locality,
                    Region = this.applePayPayment?.BillingContact?.AdministrativeArea,
                    PostalCode = this.applePayPayment?.BillingContact?.PostalCode,
                    Country = this.applePayPayment?.BillingContact?.Country,
                    FirstName = this.applePayPayment?.BillingContact?.GivenName,
                    LastName = this.applePayPayment?.BillingContact?.FamilyName,
                };
            }
            else
            {
                return new Model.PaymentOrchestratorService.Address
                {
                    AddressLine1 = this.applePayPayment?.ShippingContact?.AddressLines?.FirstOrDefault(),
                    AddressLine2 = this.applePayPayment?.ShippingContact?.AddressLines?.Skip(1).FirstOrDefault(),
                    AddressLine3 = this.applePayPayment?.ShippingContact?.AddressLines?.Skip(2).FirstOrDefault(),

                    City = this.applePayPayment?.ShippingContact?.Locality,
                    Region = this.applePayPayment?.ShippingContact?.AdministrativeArea,
                    PostalCode = this.applePayPayment?.ShippingContact?.PostalCode,
                    Country = this.applePayPayment?.ShippingContact?.Country,
                    FirstName = this.applePayPayment?.ShippingContact?.GivenName,
                    LastName = this.applePayPayment?.ShippingContact?.FamilyName,
                };
            }
        }

        /// <inheritdoc/>
        public PIDLData ExtractPaymentInstrument(AttachmentType attachmentType)
        {
            var details = new Dictionary<string, object>
            {
                { V7.Constants.CreditCardPropertyDescriptionName.AccountHolderName, this.applePayPayment?.BillingContact?.GivenName },
                { V7.Constants.CreditCardPropertyDescriptionName.AccountToken, this.applePayPayment?.Token?.Token },
                { V7.Constants.PropertyDescriptionIds.Address, new Dictionary<string, object> 
                    {
                        { 
                            V7.Constants.PropertyDescriptionIds.AddressLine1, this.applePayPayment?.BillingContact?.AddressLines?.FirstOrDefault()
                        },
                        {
                            V7.Constants.PropertyDescriptionIds.AddressLine2, this.applePayPayment?.BillingContact?.AddressLines?.Skip(1).FirstOrDefault()
                        },
                        {
                            V7.Constants.PropertyDescriptionIds.AddressLine3, this.applePayPayment?.BillingContact?.AddressLines?.Skip(2).FirstOrDefault()
                        },
                        {
                            V7.Constants.PropertyDescriptionIds.City, this.applePayPayment?.BillingContact?.Locality
                        },
                        {
                            V7.Constants.PropertyDescriptionIds.Region, this.applePayPayment?.BillingContact?.AdministrativeArea
                        },
                        {
                            V7.Constants.PropertyDescriptionIds.PostalCode, this.applePayPayment?.BillingContact?.PostalCode
                        },
                        {
                            V7.Constants.PropertyDescriptionIds.Country, this.applePayPayment?.BillingContact?.CountryCode
                        },
                    }
                }
            };

            return new PIDLData
            {
                [V7.Constants.PropertyDescriptionIds.PaymentMethodFamily] = V7.Constants.PaymentMethodFamily.ewallet.ToString(),
                [V7.Constants.PropertyDescriptionIds.PaymentMethodType] = V7.Constants.PaymentMethodType.ApplePay,
                [V7.Constants.PropertyDescriptionIds.Details] = details,
                [V7.Constants.PropertyDescriptionIds.AttachmentType] = attachmentType
            };
        }

        /// <inheritdoc/>
        public string ExtractEmailAddress()
        {
            return this.applePayPayment?.ShippingContact?.EmailAddress;
        }

        /// <inheritdoc/>
        /// TODO: delte this funciton once checkoutRequestClientActions is merged with paymentClientActions
        public Dictionary<string, object> GetButtonPayload(CheckoutRequestClientActions checkoutRequestClientActions, string partner, string actionType)
        {
            return new Dictionary<string, object>
            {
                { V7.Constants.ExpressCheckoutButtonPayloadKey.Amount, Convert.ToString(checkoutRequestClientActions?.Amount) },
                { V7.Constants.QueryParameterName.Country, checkoutRequestClientActions?.Country },
                { V7.Constants.QueryParameterName.Currency, checkoutRequestClientActions?.Currency },
                { V7.Constants.QueryParameterName.Partner, partner },
                { V7.Constants.QueryParameterName.Language, checkoutRequestClientActions?.Language },
                { V7.Constants.ExpressCheckoutButtonPayloadKey.ActionType, actionType },
            };
        }

        /// <inheritdoc/>
        public Dictionary<string, object> GetButtonPayload(PaymentRequestClientActions paymentRequestClientActions, string partner, string actionType)
        {
            return new Dictionary<string, object>
            {
                { V7.Constants.ExpressCheckoutButtonPayloadKey.Amount, Convert.ToString(paymentRequestClientActions?.Amount) },
                { V7.Constants.QueryParameterName.Country, paymentRequestClientActions?.Country },
                { V7.Constants.QueryParameterName.Currency, paymentRequestClientActions?.Currency },
                { V7.Constants.QueryParameterName.Partner, partner },
                { V7.Constants.QueryParameterName.Language, paymentRequestClientActions?.Language },
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
                { V7.Constants.QueryParameterName.Language, expressCheckoutRequest?.Language },
                { V7.Constants.ExpressCheckoutButtonPayloadKey.ActionType, actionType },
                { V7.Constants.ExpressCheckoutButtonPayloadKey.TopDomainUrl, expressCheckoutRequest?.TopDomainUrl }
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

        private ApplePayPayment DeserializeApplePayToken(string applePayToken)
        {
            if (applePayToken == null)
            {
                throw TraceCore.TraceException(this.traceActivityId, new NotSupportedException($"Apple pay token should not be null"));
            }

            ApplePayPayment applePayDate = null;

            try
            {
                applePayDate = JsonConvert.DeserializeObject<ApplePayPayment>(applePayToken);
            }
            catch
            {
                throw TraceCore.TraceException(this.traceActivityId, new FailedOperationException($"Failed to deserialize apple pay data. Token: {applePayToken}"));
            }

            return applePayDate;
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