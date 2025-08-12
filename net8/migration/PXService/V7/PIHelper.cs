// <copyright file="PIHelper.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.V7
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.Commerce.Payments.Common;
    using Microsoft.Commerce.Payments.Common.Tracing;
    using Microsoft.Commerce.Payments.Common.Web;
    using Microsoft.Commerce.Payments.PartnerSettingsModel;
    using Microsoft.Commerce.Payments.Pidl.Localization;
    using Microsoft.Commerce.Payments.PidlFactory.V7;
    using Microsoft.Commerce.Payments.PimsModel.V4;
    using Microsoft.Commerce.Payments.PXService.Model.PXInternal;
    using Microsoft.Commerce.Payments.PXService.Settings;

    public static class PIHelper
    {
        public static bool IsPayPal(this PaymentInstrument pi)
        {
            return pi != null
                && pi.PaymentMethod != null
                && IsPayPal(pi.PaymentMethod.PaymentMethodFamily, pi.PaymentMethod.PaymentMethodType);
        }

        public static bool IsPayPal(string paymentMethodFamily, string paymentMethodType)
        {
            return string.Equals(paymentMethodFamily, Constants.PaymentMethodFamily.ewallet.ToString(), StringComparison.OrdinalIgnoreCase)
                && string.Equals(paymentMethodType, Constants.PaymentMethodType.PayPal, StringComparison.OrdinalIgnoreCase);
        }

        public static bool IsVenmo(this PaymentInstrument pi)
        {
            return pi != null
                && pi.PaymentMethod != null
                && IsVenmo(pi.PaymentMethod.PaymentMethodFamily, pi.PaymentMethod.PaymentMethodType);
        }

        public static bool IsSepa(this PaymentInstrument pi)
        {
            return pi != null
                && pi.PaymentMethod != null
                && IsSepa(pi.PaymentMethod.PaymentMethodFamily, pi.PaymentMethod.PaymentMethodType);
        }

        public static bool IsPaymentMethodType(this PaymentInstrument pi, object paymentMethodFamily, string paymentMethodType)
        {
            return pi != null
                && pi.PaymentMethod != null
                && pi.PaymentMethod.IsPaymentMethodType(Convert.ToString(paymentMethodFamily), paymentMethodType);
        }

        public static bool IsVenmo(string paymentMethodFamily, string paymentMethodType)
        {
            return string.Equals(paymentMethodFamily, Constants.PaymentMethodFamily.ewallet.ToString(), StringComparison.OrdinalIgnoreCase)
                && string.Equals(paymentMethodType, Constants.PaymentMethodType.Venmo, StringComparison.OrdinalIgnoreCase);
        }

        public static bool IsSepa(string paymentMethodFamily, string paymentMethodType)
        {
            return string.Equals(paymentMethodFamily, Constants.PaymentMethodFamily.direct_debit.ToString(), StringComparison.OrdinalIgnoreCase)
                && string.Equals(paymentMethodType, Constants.PaymentMethodType.Sepa, StringComparison.OrdinalIgnoreCase);
        }

        public static bool IsUpi(string paymentMethodFamily, string paymentMethodType)
        {
            return string.Equals(paymentMethodFamily, Constants.PaymentMethodFamily.real_time_payments.ToString(), StringComparison.OrdinalIgnoreCase)
                && string.Equals(paymentMethodType, Constants.PaymentMethodType.UPI, StringComparison.OrdinalIgnoreCase);
        }

        public static bool IsUpiCommercial(string paymentMethodFamily, string paymentMethodType)
        {
            return string.Equals(paymentMethodFamily, Constants.PaymentMethodFamily.real_time_payments.ToString(), StringComparison.OrdinalIgnoreCase)
                && string.Equals(paymentMethodType, Constants.PaymentMethodType.UPICommercial, StringComparison.OrdinalIgnoreCase);
        }

        public static bool IsUpiQr(this PaymentInstrument pi)
        {
            return pi != null
                && pi.PaymentMethod != null
                && IsUpiQr(pi?.PaymentMethod);
        }

        public static bool IsGooglePay(this PaymentInstrument pi)
        {
            return pi != null
                && pi.PaymentMethod != null
                && IsGooglePay(pi?.PaymentMethod);
        }

        public static bool IsGooglePayInstancePI(this PaymentInstrument pi)
        {
            return pi != null
                && pi.PaymentMethod != null
                && IsGooglePay(pi?.PaymentMethod)
                && pi.PaymentInstrumentId.StartsWith(Constants.WalletServiceConstants.GooglePayPiidPrefix);
        }

        public static bool IsApplePay(this PaymentInstrument pi)
        {
            return pi != null
                && pi.PaymentMethod != null
                && IsApplePay(pi?.PaymentMethod);
        }

        public static bool IsCSV(this PaymentInstrument pi)
        {
            return pi != null
                && pi.PaymentMethod != null
                && IsCSV(pi?.PaymentMethod);
        }

        public static bool IsCreditCard(this PaymentInstrument pi)
        {
            return pi != null
                && pi.PaymentMethod != null
                && IsCreditCard(pi.PaymentMethod.PaymentMethodFamily);
        }

        public static bool IsCreditCard(string paymentMethodFamily)
        {
            return string.Equals(paymentMethodFamily, Constants.PaymentMethodFamily.credit_card.ToString(), StringComparison.OrdinalIgnoreCase);
        }

        public static bool IsLegacyBilldeskPayment(this PaymentInstrument pi)
        {
            return pi != null
                && pi.PaymentMethod != null
                && string.Equals(pi.PaymentMethod.PaymentMethodFamily, Constants.PaymentMethodFamily.ewallet.ToString(), StringComparison.OrdinalIgnoreCase)
                && string.Equals(pi.PaymentMethod.PaymentMethodType, Constants.PaymentMethodType.LegacyBilldeskPayment.ToString(), StringComparison.OrdinalIgnoreCase);
        }

        public static bool IsVirtualLegacyInvoice(this PaymentMethod pm)
        {
            return IsVirtualLegacyInvoice(pm.PaymentMethodFamily, pm.PaymentMethodType);
        }

        public static bool IsVirtualLegacyInvoice(string paymentMethodFamily, string paymentMethodType)
        {
            return string.Equals(paymentMethodFamily, Constants.PaymentMethodFamily.@virtual.ToString(), StringComparison.OrdinalIgnoreCase)
                && string.Equals(paymentMethodType, Constants.PaymentMethodType.LegacyInvoice, StringComparison.OrdinalIgnoreCase);
        }

        public static void SetPendingIfPayPalMIB(PaymentInstrument pi, string partner)
        {
            bool isPayPalMIB = pi.IsPayPal()
                && pi.Status.Equals(PaymentInstrumentStatus.Active)
                && pi.PaymentInstrumentDetails != null
                && string.Equals(pi.PaymentInstrumentDetails.BillingAgreementType, Constants.PayPalBillingAgreementTypes.MerchantInitiatedBilling, StringComparison.OrdinalIgnoreCase);

            if (isPayPalMIB && Constants.ThirdPartyPaymentPartners.Contains(partner))
            {
                pi.Status = PaymentInstrumentStatus.Pending;
                pi.PaymentInstrumentDetails.PendingOn = V7.Constants.PaymentInstrumentPendingOnTypes.AgreementUpdate;
            }
        }

        public static bool IsCreditCardAmex(this PaymentMethod pm)
        {
            return string.Equals(pm.PaymentMethodFamily, Constants.PaymentMethodFamily.credit_card.ToString(), StringComparison.OrdinalIgnoreCase)
                    && string.Equals(pm.PaymentMethodType, Constants.PaymentMethodType.CreditCardAmericanExpress, StringComparison.OrdinalIgnoreCase);
        }

        public static bool IsCreditCardVisa(this PaymentMethod pm)
        {
            return string.Equals(pm.PaymentMethodFamily, Constants.PaymentMethodFamily.credit_card.ToString(), StringComparison.OrdinalIgnoreCase)
                    && string.Equals(pm.PaymentMethodType, Constants.PaymentMethodType.CreditCardVisa, StringComparison.OrdinalIgnoreCase);
        }

        public static bool IsCreditCardMasterCard(this PaymentMethod pm)
        {
            return string.Equals(pm.PaymentMethodFamily, Constants.PaymentMethodFamily.credit_card.ToString(), StringComparison.OrdinalIgnoreCase)
                    && string.Equals(pm.PaymentMethodType, Constants.PaymentMethodType.CreditCardMasterCard, StringComparison.OrdinalIgnoreCase);
        }

        public static bool IsDirectDebitSepa(this PaymentMethod pm)
        {
            return string.Equals(pm.PaymentMethodFamily, Constants.PaymentMethodFamily.direct_debit.ToString(), StringComparison.OrdinalIgnoreCase)
                && string.Equals(pm.PaymentMethodType, Constants.PaymentMethodType.Sepa, StringComparison.OrdinalIgnoreCase);
        }

        public static bool IsOnlineBankTransferPaySafe(this PaymentMethod pm)
        {
            return string.Equals(pm.PaymentMethodFamily, Constants.PaymentMethodFamily.online_bank_transfer.ToString(), StringComparison.OrdinalIgnoreCase)
                && string.Equals(pm.PaymentMethodType, Constants.PaymentMethodType.Paysafecard, StringComparison.OrdinalIgnoreCase);
        }

        public static bool IsInvoiceCreditKlarna(this PaymentMethod pm)
        {
            return string.Equals(pm.PaymentMethodFamily, Constants.PaymentMethodFamily.invoice_credit.ToString(), StringComparison.OrdinalIgnoreCase)
                && string.Equals(pm.PaymentMethodType, Constants.PaymentMethodType.Klarna, StringComparison.OrdinalIgnoreCase);
        }

        public static bool IsAch(this PaymentMethod pm)
        {
            return string.Equals(pm.PaymentMethodFamily, Constants.PaymentMethodFamily.direct_debit.ToString(), StringComparison.OrdinalIgnoreCase)
                && string.Equals(pm.PaymentMethodType, Constants.PaymentMethodType.Ach, StringComparison.OrdinalIgnoreCase);
        }

        public static bool IsChinaUnionPay(string paymentMethodFamily, string paymentMethodType)
        {
            bool isCardFamily = string.Equals(paymentMethodFamily, Constants.PaymentMethodFamily.credit_card.ToString(), StringComparison.OrdinalIgnoreCase);
            bool isCUPCreditType = string.Equals(paymentMethodType, Constants.PaymentMethodType.UnionPayCreditCard.ToString(), StringComparison.OrdinalIgnoreCase);
            bool isCUPDebitType = string.Equals(paymentMethodType, Constants.PaymentMethodType.UnionPayDebitCard.ToString(), StringComparison.OrdinalIgnoreCase);
            return isCardFamily && (isCUPCreditType || isCUPDebitType);
        }

        public static bool IsPaymentMethodType(this PaymentMethod pm, string paymentMethodFamily, string paymentMethodType)
        {
            return string.Equals(pm.PaymentMethodFamily, paymentMethodFamily, StringComparison.OrdinalIgnoreCase)
                    && string.Equals(pm.PaymentMethodType, paymentMethodType, StringComparison.OrdinalIgnoreCase);
        }

        public static bool IsCheck(this PaymentMethod pm)
        {
            return string.Equals(pm.PaymentMethodFamily, Constants.PaymentMethodFamily.offline_bank_transfer.ToString(), StringComparison.OrdinalIgnoreCase)
                    && string.Equals(pm.PaymentMethodType, Constants.PaymentMethodType.Check, StringComparison.OrdinalIgnoreCase);
        }

        public static bool IsVenmo(this PaymentMethod pm)
        {
            return string.Equals(pm.PaymentMethodFamily, Constants.PaymentMethodFamily.ewallet.ToString(), StringComparison.OrdinalIgnoreCase)
                    && string.Equals(pm.PaymentMethodType, Constants.PaymentMethodType.Venmo, StringComparison.OrdinalIgnoreCase);
        }

        public static bool IsUpi(this PaymentMethod pm)
        {
            return string.Equals(pm.PaymentMethodFamily, Constants.PaymentMethodFamily.real_time_payments.ToString(), StringComparison.OrdinalIgnoreCase)
                    && string.Equals(pm.PaymentMethodType, Constants.PaymentMethodType.UPI, StringComparison.OrdinalIgnoreCase);
        }

        public static bool IsUpiQr(this PaymentMethod pm)
        {
            return string.Equals(pm.PaymentMethodFamily, Constants.PaymentMethodFamily.real_time_payments.ToString(), StringComparison.OrdinalIgnoreCase)
                    && string.Equals(pm.PaymentMethodType, Constants.PaymentMethodType.UPIQr, StringComparison.OrdinalIgnoreCase);
        }

        public static bool IsGooglePay(this PaymentMethod pm)
        {
            return string.Equals(pm.PaymentMethodFamily, Constants.PaymentMethodFamily.ewallet.ToString(), StringComparison.OrdinalIgnoreCase)
                    && string.Equals(pm.PaymentMethodType, Constants.PaymentMethodType.GooglePay, StringComparison.OrdinalIgnoreCase);
        }

        public static bool IsApplePay(this PaymentMethod pm)
        {
            return string.Equals(pm.PaymentMethodFamily, Constants.PaymentMethodFamily.ewallet.ToString(), StringComparison.OrdinalIgnoreCase)
                    && string.Equals(pm.PaymentMethodType, Constants.PaymentMethodType.ApplePay, StringComparison.OrdinalIgnoreCase);
        }

        public static bool IsUpiQrCommercial(this PaymentMethod pm)
        {
            return string.Equals(pm.PaymentMethodFamily, Constants.PaymentMethodFamily.real_time_payments.ToString(), StringComparison.OrdinalIgnoreCase)
                    && string.Equals(pm.PaymentMethodType, Constants.PaymentMethodType.UPIQrCommercial, StringComparison.OrdinalIgnoreCase);
        }

        public static bool IsUpiCommercial(this PaymentMethod pm)
        {
            return string.Equals(pm.PaymentMethodFamily, Constants.PaymentMethodFamily.real_time_payments.ToString(), StringComparison.OrdinalIgnoreCase)
                    && string.Equals(pm.PaymentMethodType, Constants.PaymentMethodType.UPICommercial, StringComparison.OrdinalIgnoreCase);
        }

        public static bool IsCreditCardRupay(this PaymentMethod pm)
        {
            return string.Equals(pm.PaymentMethodFamily, Constants.PaymentMethodFamily.credit_card.ToString(), StringComparison.OrdinalIgnoreCase)
                    && string.Equals(pm.PaymentMethodType, Constants.PaymentMethodType.CreditCardRupay, StringComparison.OrdinalIgnoreCase);
        }

        public static bool IsCSV(this PaymentMethod pm)
        {
            return string.Equals(pm.PaymentMethodFamily, Constants.PaymentMethodFamily.ewallet.ToString(), StringComparison.OrdinalIgnoreCase)
                    && string.Equals(pm.PaymentMethodType, Constants.EwalletType.stored_value.ToString(), StringComparison.OrdinalIgnoreCase);
        }

        public static bool IsQrCodeValidSession(QRCodeSecondScreenSession qrCodePaymentSessionData)
        {
            if (qrCodePaymentSessionData != null
                && PXCommon.Constants.PartnerGroups.IsXboxNativePartner(qrCodePaymentSessionData.Partner)
                && string.Equals(qrCodePaymentSessionData.Operation, Constants.Operations.Add, StringComparison.OrdinalIgnoreCase)
                && qrCodePaymentSessionData.Status == PimsModel.V4.PaymentInstrumentStatus.Pending)
            {
                // Catch edge cases
                if (qrCodePaymentSessionData.UseCount > Constants.AddPIQrCode.MaxSessionIdRetry)
                {
                    throw new ValidationException(ErrorCode.RequestFailed, Constants.QRCodeErrorMessages.RetryMax);
                }
                else if ((qrCodePaymentSessionData.QrCodeCreatedTime.AddMinutes(Constants.AddPIQrCode.QrCodeSessionTimeoutInMinutes) < DateTime.UtcNow && qrCodePaymentSessionData.FormRenderedTime.Date.ToString().Contains("0001"))
                    || (!qrCodePaymentSessionData.FormRenderedTime.Date.ToString().Contains("0001") && qrCodePaymentSessionData.FormRenderedTime.AddMinutes(Constants.AddPIQrCode.AnonymousSecondScreenFormRenderedTimeoutInMinutes) < DateTime.UtcNow))
                {
                    throw new ValidationException(ErrorCode.RequestFailed, Constants.QRCodeErrorMessages.Expired);
                }

                return true;
            }

            return false;
        }

        public static void AddDefaultDisplayName(this PaymentInstrument pi, string partner, EventTraceActivity traceActivityId, string country = null, List<string> flightNames = null)
        {
            try
            {
                if (pi.PaymentInstrumentDetails == null)
                {
                    pi.PaymentInstrumentDetails = new PaymentInstrumentDetails();
                }

                pi.PaymentInstrumentDetails.DefaultDisplayName = PaymentSelectionHelper.BuildPiDefaultDisplayName(pi, partner, country: country, flightNames: flightNames);
            }
            catch (Exception ex)
            {
                SllWebLogger.TracePXServiceException("PIHelper.AddDefaultDisplayName: " + ex.ToString(), traceActivityId);
            }
        }

        public static void OverridePMDisplayName(PaymentMethod pm, string partner, string language, string country = null, PaymentExperienceSetting setting = null)
        {
            if (pm == null || partner == null)
            {
                return;
            }

            bool isCheck = pm.IsCheck();
            bool isCommercialStores = string.Equals(partner, Constants.PartnerName.CommercialStores, StringComparison.OrdinalIgnoreCase);
            bool isOverrideCheckToWireTransferEnabled = PartnerSettingsHelper.IsFeatureEnabledUsingPartnerSettings(PartnerSettingsHelper.Features.OverrideCheckDisplayNameToWireTransfer, country, setting);

            if (isCheck && (isCommercialStores || isOverrideCheckToWireTransferEnabled))
            {
                pm.Display.Name = LocalizationRepository.Instance.GetLocalizedString("Wire Transfer", language);
            }
        }

        public static async Task<bool> HasAnyStoredPI(PXServiceSettings pxSettings, string accountId, string partner, string country, string language, EventTraceActivity traceActivityId, List<string> exposedFlightFeatures)
        {
            // Get user's PIs
            PaymentInstrument[] paymentInstruments = await GetUserPaymentInstruments(pxSettings, accountId, partner, country, language, traceActivityId, exposedFlightFeatures: exposedFlightFeatures);

            // Check if the user has any stored PIs
            bool hasAnyStoredPI = paymentInstruments?.Any(pi => pi.PaymentMethod.Properties.IsNonStoredPaymentMethod == false) ?? false;

            return hasAnyStoredPI;
        }

        public static async Task<PaymentInstrument> GetCSVPI(PXServiceSettings pxSettings, string accountId, string partner, string country, string language, EventTraceActivity traceActivityId, List<string> exposedFlightFeatures)
        {
            // Get user's PIs
            PaymentInstrument[] paymentInstruments = await GetUserPaymentInstruments(pxSettings, accountId, partner, country, language, traceActivityId, exposedFlightFeatures: exposedFlightFeatures);

            // Get CSV PI details
            var csvPI = paymentInstruments.FirstOrDefault(pi => pi.IsCSV());

            return csvPI;
        }

        private static async Task<PaymentInstrument[]> GetUserPaymentInstruments(PXServiceSettings pxSettings, string accountId, string partner, string country, string language, EventTraceActivity traceActivityId, List<string> exposedFlightFeatures)
        {
            string[] statusList = new string[] { Constants.PaymentInstrumentStatus.Active };

            List<KeyValuePair<string, string>> queryParams = new List<KeyValuePair<string, string>>()
                {
                    new KeyValuePair<string, string>(Constants.QueryParameterName.Language, language),
                    new KeyValuePair<string, string>(Constants.QueryParameterName.Country, country)
                };

            // Get user's PIs
            PaymentInstrument[] paymentInstruments = await pxSettings.PIMSAccessor.ListPaymentInstrument(accountId, 0, statusList, traceActivityId, queryParams, partner, exposedFlightFeatures: exposedFlightFeatures, country: country, language: language);

            return paymentInstruments;
        }
    }
}
