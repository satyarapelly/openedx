// <copyright file="IExternalPaymentTokenTransformer.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.V7
{
    using System.Collections.Generic;
    using Microsoft.Commerce.Payments.PidlFactory.V7;
    using Microsoft.Commerce.Payments.PimsModel.V4;
    using Microsoft.Commerce.Payments.PXService.Model.PaymentOrchestratorService;
    using Microsoft.Commerce.Payments.PXService.Model.PXInternal;
    using Address = Model.PaymentOrchestratorService.Address;

    public interface IExternalPaymentTokenTransformer
    {
        /// <summary>
        /// Extracts and transforms an address into the Jarvis address format.
        /// </summary>
        /// <param name="addressType">The type of address to extract (e.g., billing or shipping).</param>
        /// <returns>An AddressV3 object representing the transformed address.</returns>
        AddressInfoV3 ExtractAddressInfoV3(string addressType = V7.Constants.AddressTypes.Billing);

        /// <summary>
        /// Extracts and transforms an address into the Jarvis address format.
        /// </summary>
        /// <param name="addressType">The type of address to extract (e.g., billing or shipping).</param>
        /// <returns>An AddressV3 object representing the transformed address.</returns>
        Address ExtractAddress(string addressType = V7.Constants.AddressTypes.Billing);

        /// <summary>
        /// Extracts and transforms the payment information into a PIMS PI input payload.
        /// </summary>
        /// <param name="attachmentType">The attchment type (e.g., Wallet or Standalone).</param>
        /// <returns>A PaymentInstrumentRequest object representing the transformed payment data.</returns>
        PIDLData ExtractPaymentInstrument(AttachmentType attachmentType = AttachmentType.Standalone);

        /// <summary>
        /// Extracts email address
        /// </summary>
        /// <returns>Email address</returns>
        string ExtractEmailAddress();

        /// <summary>
        /// Extracts button payload from checkout request client action
        /// </summary>
        /// <param name="checkoutRequestClientActions">Checkout reqeuet client action</param>
        /// <param name="partner">Name of the partenr</param>
        /// <param name="actionType">Button action type e.g. trigger submit/success</param>
        /// <returns>A PaymentInstrumentRequest object representing the transformed payment data.</returns>
        Dictionary<string, object> GetButtonPayload(CheckoutRequestClientActions checkoutRequestClientActions, string partner, string actionType);

        /// <summary>
        /// Extracts button payload from checkout request client action
        /// </summary>
        /// <param name="paymentRequestClientActions">Checkout reqeuet client action</param>
        /// <param name="partner">Name of the partenr</param>
        /// <param name="actionType">Button action type e.g. trigger submit/success</param>
        /// <returns>A PaymentInstrumentRequest object representing the transformed payment data.</returns>
        Dictionary<string, object> GetButtonPayload(PaymentRequestClientActions paymentRequestClientActions, string partner, string actionType);

        /// <summary>
        /// Extracts button payload from express checkout request data
        /// </summary>
        /// <param name="expressCheckoutRequest">Express cehckout request paylaod</param>
        /// <param name="partner">Name of the partenr</param>
        /// <param name="actionType">Button action type e.g. trigger submit/success</param>
        /// <returns>A PaymentInstrumentRequest object representing the transformed payment data.</returns>
        Dictionary<string, object> GetButtonPayload(ExpressCheckoutRequest expressCheckoutRequest, string partner, string actionType);
    }
}