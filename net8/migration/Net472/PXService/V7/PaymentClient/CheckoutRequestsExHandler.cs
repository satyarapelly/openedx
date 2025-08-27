// <copyright file="CheckoutRequestsExHandler.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.V7.PaymentClient
{
    using System;
    using System.Collections.Generic;
    using Microsoft.Commerce.Payments.Common.Tracing;
    using Microsoft.Commerce.Payments.PidlFactory.V7;
    using Microsoft.Commerce.Payments.PimsModel.V4;
    using Microsoft.Commerce.Payments.PXCommon;
    using Microsoft.Commerce.Payments.PXService.Model.PaymentOrchestratorService;
    using Microsoft.Commerce.Tracing;

    public class CheckoutRequestsExHandler
    {
        public static Address ConvertPIDLDataToPOAddress(PIDLData pidlAddress)
        {
            if (pidlAddress == null)
            {
                throw new ArgumentNullException(nameof(pidlAddress));
            }

            PXAddressV3Info userEnteredAddress = new PXAddressV3Info(pidlAddress);

            Address address = new Address
            {
                AddressLine1 = userEnteredAddress.AddressLine1,
                AddressLine2 = userEnteredAddress.AddressLine2,
                AddressLine3 = userEnteredAddress.AddressLine3,
                City = userEnteredAddress.City,
                Region = userEnteredAddress.Region,
                PostalCode = userEnteredAddress.PostalCode,
                Country = userEnteredAddress.Country,
                PhoneNumber = userEnteredAddress.PhoneNumber,
                District = userEnteredAddress.District,
            };

            return address;
        }

        public static Address ConvertPIAddressToPOAddress(PIDLData pi)
        {
            if (pi == null)
            {
                throw new ArgumentNullException(nameof(pi));
            }

            Address address = new Address
            {
                AddressLine1 = pi.TryGetPropertyValue("details.address.address_line1"),
                AddressLine2 = pi.TryGetPropertyValue("details.address.address_line2"),
                AddressLine3 = pi.TryGetPropertyValue("details.address.address_line3"),
                City = pi.TryGetPropertyValue("details.address.city"),
                Region = pi.TryGetPropertyValue("details.address.region"),
                PostalCode = pi.TryGetPropertyValue("details.address.postal_code"),
                Country = pi.TryGetPropertyValue("details.address.country"),
                District = pi.TryGetPropertyValue("details.address.district"),
            };

            return address;
        }

        //// TODO: Delete this function after cr and pr are merged
        public static MergeDataActionContext CreateMergeDataActionContextForAttachAddress(CheckoutRequestClientActions checkoutRequest)
        {
            string subTotal = CurrencyHelper.FormatCurrency(checkoutRequest.Country, checkoutRequest.Language, checkoutRequest.SubTotalAmount, checkoutRequest.Currency);
            string taxAmount = CurrencyHelper.FormatCurrency(checkoutRequest.Country, checkoutRequest.Language, checkoutRequest.TaxAmount, checkoutRequest.Currency);
            string amount = CurrencyHelper.FormatCurrency(checkoutRequest.Country, checkoutRequest.Language, checkoutRequest.Amount, checkoutRequest.Currency);

            var mergeActionPayload = new
            {
                cart_subtotal = subTotal,
                cart_tax = taxAmount,
                cart_total = amount,
            };

            MergeDataActionContext actionContext = new MergeDataActionContext()
            {
                Explicit = true,
                Payload = mergeActionPayload
            };

            return actionContext;
        }

        public static MergeDataActionContext CreateMergeDataActionContextForAttachAddress(PaymentRequestClientActions paymentRequest)
        {
            string subTotal = CurrencyHelper.FormatCurrency(paymentRequest.Country, paymentRequest.Language, paymentRequest.SubTotalAmount, paymentRequest.Currency);
            string taxAmount = CurrencyHelper.FormatCurrency(paymentRequest.Country, paymentRequest.Language, paymentRequest.TaxAmount, paymentRequest.Currency);
            string amount = CurrencyHelper.FormatCurrency(paymentRequest.Country, paymentRequest.Language, paymentRequest.Amount, paymentRequest.Currency);

            var mergeActionPayload = new
            {
                cart_subtotal = subTotal,
                cart_tax = taxAmount,
                cart_total = amount,
            };

            MergeDataActionContext actionContext = new MergeDataActionContext()
            {
                Explicit = true,
                Payload = mergeActionPayload
            };

            return actionContext;
        }

        public static PXCommon.ClientAction CreateClientActionForPostPI(string piid)
        {
            PXCommon.ClientAction clientAction = new PXCommon.ClientAction(
                PXCommon.ClientActionType.MergeData,
                new MergeDataActionContext()
                {
                    Explicit = true,
                    Payload = new
                    {
                        selected_PIID = piid,
                    }
                });

            clientAction.NextAction = new PXCommon.ClientAction(PXCommon.ClientActionType.None);

            return clientAction;
        }
    }
}