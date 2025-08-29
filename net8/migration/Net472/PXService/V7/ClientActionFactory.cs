// <copyright file="ClientActionFactory.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.V7
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.Commerce.Payments.Common.Tracing;
    using Microsoft.Commerce.Payments.PartnerSettingsModel;
    using Microsoft.Commerce.Payments.PidlFactory.V7;
    using Microsoft.Commerce.Payments.PidlFactory.V7.PIDLGeneration;
    using Microsoft.Commerce.Payments.PidlModel.V7;
    using Microsoft.Commerce.Payments.PimsModel.V4;
    using Microsoft.Commerce.Payments.PXCommon;
    using Microsoft.Commerce.Tracing;
    using PXCommonConstants = Microsoft.Commerce.Payments.PXCommon.Constants;

    /// <summary>
    /// The class responsible for creating client actions that needs to be associated with a given PI
    /// </summary>
    public class ClientActionFactory
    {
        public static void AddProfileAddressClientActionToPaymentInstrument(PaymentInstrument paymentInstrument, string country, string type, string language, string partner, bool primaryResource, AccountProfile profile, bool overrideJarvisVersionToV3, EventTraceActivity traceActivityId, List<string> exposedFlightFeatures, PaymentExperienceSetting setting = null)
        {
            ClientAction clientAction;
            List<PIDLResource> profileAddressPidls;
            BuildProfileAddressDescriptionPidls(paymentInstrument, country, type, language, partner, primaryResource, overrideJarvisVersionToV3, traceActivityId, exposedFlightFeatures, setting, out clientAction, out profileAddressPidls);
            PIDLResourceFactory.AddSecondarySubmitAddressContext(profileAddressPidls, profile, partner, country: country, setting: setting);
            clientAction.Context = profileAddressPidls;
            paymentInstrument.ClientAction = clientAction;
        }

        public static void AddProfileV3AddressClientActionToPaymentInstrument(PaymentInstrument paymentInstrument, string country, string type, string language, string partner, bool primaryResource, AccountProfileV3 profileV3, EventTraceActivity traceActivityId, List<string> exposedFlightFeatures, PaymentExperienceSetting setting = null)
        {
            ClientAction clientAction;
            List<PIDLResource> profileAddressPidls;
            BuildProfileAddressDescriptionPidls(paymentInstrument, country, type, language, partner, primaryResource, true, traceActivityId, exposedFlightFeatures, setting, out clientAction, out profileAddressPidls);

            Dictionary<string, string> profileV3Headers = new Dictionary<string, string>();
            profileV3Headers.Add(AccountService.V7.Constants.AccountV3ExtendedHttpHeaders.Etag, profileV3?.Etag);
            profileV3Headers.Add(AccountService.V7.Constants.AccountV3ExtendedHttpHeaders.IfMatch, profileV3?.Etag);

            PIDLResourceFactory.AddSecondarySubmitAddressV3Context(profileAddressPidls, profileV3, partner, profileV3Headers, country: country, setting: setting);
            clientAction.Context = profileAddressPidls;
            paymentInstrument.ClientAction = clientAction;
        }

        public static void AddClientActionToPaymentInstrument(PaymentInstrument paymentInstrument, string accountId, string language, string partner, string classicProduct, string billableAccountId, EventTraceActivity traceActivityId, string pidlBaseUrl = null, string requestType = null, bool completePrerequisites = false, string country = null, string emailAddress = null, string scenario = null, string sessionQueryUrl = null, List<string> exposedFlightFeatures = null, string sessionId = null, string shortUrl = null, PaymentExperienceSetting setting = null)
        {
            if (paymentInstrument == null)
            {
                throw TraceCore.TraceException<PXServiceException>(traceActivityId, new PXServiceException(Constants.PXServiceErrorCodes.ArgumentIsNull, "paymentInstrument"));
            }

            // For Sepa, it might require picv challenge even PI status is active
            // Added the conditions check for sepa picv.
            // For other PIs, only require clientAction if PI status is pending
            if ((paymentInstrument.Status != PaymentInstrumentStatus.Pending && !IsSepa(paymentInstrument)) ||
                (string.Equals(paymentInstrument.Status.ToString(), Constants.PaymentInstrumentStatus.Active, StringComparison.OrdinalIgnoreCase) && IsSepa(paymentInstrument) && paymentInstrument.PaymentInstrumentDetails.PicvDetails == null))
            {
                return;
            }

            // For Wallet, only attempt to add client actions to digitized credit cards.  Display descriptions for other PI types
            // have not been defined for Wallet (as Wallet does not deal with other PI types)
            // TODO Bug 1704671:[PX AP] Ensure default partner has the superset of all display descriptions
            // TODO Bug 1704670:[PX AP] Fallback to default partner if display descriptions do not exist for a specific partner
            // TODO Bug- 44263093 : The && !IsDigitizedCard(paymentInstrument) condition is being removed from the if statement. The entire if statement will be removed when the wallet partner is removed
            if (string.Equals(partner, Constants.PartnerName.Wallet, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            PaymentInstrumentDetails paymentInstrumentDetails = paymentInstrument.PaymentInstrumentDetails;

            if (paymentInstrumentDetails == null)
            {
                throw TraceCore.TraceException<PXServiceException>(traceActivityId, new PXServiceException(Constants.PXServiceErrorCodes.ArgumentIsNull, "paymentInstrument.PaymentInstrumentDetails"));
            }

            if (setting != null && setting.RedirectionPattern != null && IsSupportedPSSRedirectPI(paymentInstrument, country))
            {
                string resourceId = $"{paymentInstrument.PaymentMethod.PaymentMethodFamily}.{paymentInstrument.PaymentMethod.PaymentMethodType}";
                PIDLGeneratorContext context = new PIDLGeneratorContext(
                    paymentInstrument,
                    accountId,
                    country,
                    originalPartner: partner,
                    TemplateHelper.GetSettingTemplate(partner, setting, Constants.DescriptionTypes.PaymentMethodDescription, resourceId),
                    Constants.DescriptionTypes.PaymentMethodDescription,
                    Constants.Operations.Add,
                    resourceId,
                    scenario,
                    language,
                    classicProduct,
                    sessionQueryUrl,
                    completePrerequisites,
                    requestType,
                    billableAccountId,
                    emailAddress,
                    exposedFlightFeatures,
                    sessionId,
                    pidlBaseUrl,
                    GetPIDLGenerationContextDescriptionType(paymentInstrument, requestType, country, setting),
                    shortUrl,
                    setting,
                    traceActivityId);

                ClientAction clientAction = PIDLGenerator.Generate<ClientAction>(PIDLResourceFactory.ClientActionGenerationFactory, context);
                if (clientAction != null)
                {
                    paymentInstrument.ClientAction = clientAction;
                    return;
                }
            }

            // The expected behavior is pendingOn to be always set when the status is pending
            // Created bug VSTS 1607386 on PIMS side because the pendingOn is absent for case of redirect
            // The expected behavior is that PIMS should return the redirect Url for pending PIs.
            // But for List PI PIMS is not returning a redirect Url
            // Bug 1607369 filed for this
            // TODO : Once PIMS fixes the contract add pendingOn check and throw exception in the else condition (Bug 1607346)
            if (IsIdealBillingAgreement(paymentInstrument))
            {
                if (!string.IsNullOrEmpty(paymentInstrumentDetails.RedirectUrl))
                {
                    ////RedirectionServiceLink redirectLink = GetRedirectionServiceLink(paymentInstrument);
                    ClientAction clientAction = new ClientAction(ClientActionType.Redirect);
                    clientAction.Context = paymentInstrument.PaymentInstrumentDetails.RedirectUrl;
                    ////clientAction.Context = redirectLink;

                    // check if the partner has a redirection pattern defined in the partner settings
                    string redirectionPattern = TemplateHelper.GetRedirectionPatternFromPartnerSetting(setting, Constants.DescriptionTypes.PaymentMethodDescription, $"{paymentInstrument.PaymentMethod.PaymentMethodFamily}.{paymentInstrument.PaymentMethod.PaymentMethodType}");

                    if ((redirectionPattern != null && !redirectionPattern.Contains(Constants.RedirectionPatterns.Inline.ToLowerInvariant())) || (!Constants.InlinePartners.Contains(partner.ToLowerInvariant()) && redirectionPattern == null))
                    {
                        clientAction.RedirectPidl = PIDLResourceFactory.Instance.GetRedirectPidlForPI(paymentInstrument, Constants.PidlResourceDescriptionType.IdealBillingAgreementRedirectStaticPidl, language, partner, completePrerequisites, country, setting: setting);
                    }

                    paymentInstrument.ClientAction = clientAction;
                    return;
                }
                else
                {
                    return;
                }
            }
            else if (IsNonSimMobi(paymentInstrument) || IsChinaUnionPay(paymentInstrument) || IsAlipay(paymentInstrument))
            {
                if (!string.IsNullOrEmpty(paymentInstrumentDetails.PendingOn))
                {
                    if (string.Equals(paymentInstrumentDetails.PendingOn, Constants.PaymentInstrumentPendingOnTypes.Sms, StringComparison.OrdinalIgnoreCase))
                    {
                        ClientAction clientAction = new ClientAction(ClientActionType.Pidl);
                        clientAction.Context = PIDLResourceFactory.Instance.GetSmsChallengeDescriptionForPI(paymentInstrument, language, partner, classicProduct, billableAccountId, emailAddress, completePrerequisites, country, setting: setting, exposedFlightFeatures: exposedFlightFeatures);
                        paymentInstrument.ClientAction = clientAction;
                        return;
                    }
                    else if (IsAlipay(paymentInstrument) && string.Equals(paymentInstrumentDetails.PendingOn, Constants.PaymentInstrumentPendingOnTypes.Notification, StringComparison.OrdinalIgnoreCase))
                    {
                        ClientAction clientAction = new ClientAction(ClientActionType.Pidl);
                        clientAction.Context = PIDLResourceFactory.Instance.GetQrCodeChallengeDescriptionForPI(paymentInstrument, language, Constants.ChallengeDescriptionTypes.AlipayQrCode, partner, classicProduct, billableAccountId, emailAddress, completePrerequisites, country, setting: setting);
                        paymentInstrument.ClientAction = clientAction;
                        return;
                    }
                    else
                    {
                        throw TraceCore.TraceException<IntegrationException>(
                            traceActivityId,
                            new IntegrationException(
                                PXCommonConstants.ServiceNames.InstrumentManagementService,
                                string.Format("The state of PI was expected to be pending on SMS. Actual state {0}", paymentInstrumentDetails.PendingOn),
                                Constants.PXServiceIntegrationErrorCodes.InvalidPendingOnType));
                    }
                }
                else
                {
                    throw TraceCore.TraceException<IntegrationException>(
                        traceActivityId,
                        new IntegrationException(
                                PXCommonConstants.ServiceNames.InstrumentManagementService,
                                "The state of the PI is set to pending but the pendingOn is null",
                                Constants.PXServiceIntegrationErrorCodes.InvalidPendingOnType));
                }
            }
            else if (PXCommon.Constants.PartnerGroups.IsXboxNativePartner(partner) && IsKakaopay(paymentInstrument))
            {
                AddClientActionToKakaopayRequest(paymentInstrument, language, partner, requestType, completePrerequisites, country, scenario, emailAddress, exposedFlightFeatures: exposedFlightFeatures, sessionId: sessionId, shortUrl: shortUrl);
            }
            else if (IsGenericRedirect(paymentInstrument))
            {
                AddClientActionToGenericRedirectRequest(paymentInstrument, language, partner, requestType, completePrerequisites, country, scenario);
            }
            else if (paymentInstrument.IsPayPal())
            {
                AddClientActionToPaypalRequest(paymentInstrument, language, partner, requestType, completePrerequisites, country, scenario, emailAddress, exposedFlightFeatures: exposedFlightFeatures, sessionId: sessionId, shortUrl: shortUrl?.ToString(), setting: setting);
            }
            else if (paymentInstrument.IsVenmo() && (PIDLResourceFactory.IsTemplateInList(partner, setting, Constants.DescriptionTypes.PaymentMethodDescription, $"{paymentInstrument.PaymentMethod.PaymentMethodFamily}.{paymentInstrument.PaymentMethod.PaymentMethodType}") || PXCommon.Constants.PartnerGroups.IsVenmoEnabledPartner(partner)) && country != null && string.Equals(country, "us", StringComparison.OrdinalIgnoreCase))
            {
                AddClientActionToVenmoRequest(paymentInstrument, language, partner, requestType, completePrerequisites, country, scenario, emailAddress, exposedFlightFeatures: exposedFlightFeatures, sessionId: sessionId, setting: setting, shortUrl: shortUrl?.ToString());
            }
            else if (IsAch(paymentInstrument))
            {
                AddClientActionToAchRequest(paymentInstrument, language, partner, classicProduct, billableAccountId, traceActivityId, requestType);
            }
            else if (IsSepa(paymentInstrument))
            {
                AddClientActionToSepaRequest(paymentInstrument, language, partner, classicProduct, billableAccountId, traceActivityId, exposedFlightFeatures, setting, completePrerequisites, country, scenario);
            }
            else if (IsCreditCard(paymentInstrument) && string.Equals(country, "in", StringComparison.OrdinalIgnoreCase))
            {
                AddClientActionToIndiaCCRequest(paymentInstrument, language, partner, classicProduct, completePrerequisites, country, scenario, sessionQueryUrl, requestType, billableAccountId, emailAddress, exposedFlightFeatures, sessionId, pidlBaseUrl, setting: setting);
            }
            else
            {
                return;
            }
        }

        public static string GetPIDLGenerationContextDescriptionType(PaymentInstrument paymentInstrument, string requestType, string country, PaymentExperienceSetting setting)
        {
            string paymentMethodType = $"{paymentInstrument.PaymentMethod.PaymentMethodFamily}.{paymentInstrument.PaymentMethod.PaymentMethodType}";
            string redirectionPattern = TemplateHelper.GetRedirectionPatternFromPartnerSetting(setting, Constants.DescriptionTypes.PaymentMethodDescription, paymentMethodType);

            if (IsCreditCard(paymentInstrument) && string.Equals(country, "in", StringComparison.OrdinalIgnoreCase))
            {
                return Constants.ChallengeDescriptionTypes.ThreeDSOneQrCode;
            }

            if (paymentInstrument.IsVenmo())
            {
                if (string.Equals(redirectionPattern, Constants.RedirectionPatterns.QRCode, StringComparison.OrdinalIgnoreCase))
                {
                    return Constants.ChallengeDescriptionTypes.VenmoQrCode;
                }
                else if (string.Equals(redirectionPattern, Constants.RedirectionPatterns.FullPage, StringComparison.OrdinalIgnoreCase))
                {
                    if (string.Equals(requestType, Constants.RequestType.AddPI, StringComparison.OrdinalIgnoreCase))
                    {
                        return Constants.PidlResourceDescriptionType.VenmoRedirectStaticPidl;
                    }
                    else if (string.Equals(requestType, Constants.RequestType.GetPI, StringComparison.OrdinalIgnoreCase))
                    {
                        return Constants.PidlResourceDescriptionType.VenmoRetryStaticPidl;
                    }
                }
            }

            if (paymentInstrument.IsPayPal())
            {
                if (string.Equals(redirectionPattern, Constants.RedirectionPatterns.QRCode, StringComparison.OrdinalIgnoreCase))
                {
                    return Constants.ChallengeDescriptionTypes.PaypalQrCode;
                }
                else if (string.Equals(redirectionPattern, Constants.RedirectionPatterns.FullPage, StringComparison.OrdinalIgnoreCase))
                {
                    if (string.Equals(requestType, Constants.RequestType.AddPI, StringComparison.OrdinalIgnoreCase))
                    {
                        return Constants.PidlResourceDescriptionType.PaypalRedirectStaticPidl;
                    }
                    else if (string.Equals(requestType, Constants.RequestType.GetPI, StringComparison.OrdinalIgnoreCase))
                    {
                        return Constants.PidlResourceDescriptionType.PaypalRetryStaticPidl;
                    }
                }
            }

            if (paymentInstrument.IsSepa())
            {
                if (string.Equals(paymentInstrument.Status.ToString(), Constants.PaymentInstrumentStatus.Pending, StringComparison.OrdinalIgnoreCase))
                {
                    if (string.Equals(redirectionPattern, Constants.RedirectionPatterns.FullPage, StringComparison.OrdinalIgnoreCase))
                    {
                        return Constants.PidlResourceDescriptionType.SepaPicVStatic;
                    }
                }
                else if (string.Equals(paymentInstrument.Status.ToString(), Constants.PaymentInstrumentStatus.Active, StringComparison.OrdinalIgnoreCase))
                {
                    if (paymentInstrument.PaymentInstrumentDetails.PicvDetails != null && string.Equals(paymentInstrument.PaymentInstrumentDetails.PicvDetails.Status, Constants.PicvStatus.InProgress, StringComparison.OrdinalIgnoreCase))
                    {
                        return Constants.PidlResourceDescriptionType.SepaPicVChallenge;
                    }
                }
            }

            if (string.Equals(redirectionPattern, Constants.RedirectionPatterns.QRCode, StringComparison.OrdinalIgnoreCase))
            {
                return Constants.ChallengeDescriptionTypes.GenericQrCode;
            }
            else if (string.Equals(redirectionPattern, Constants.RedirectionPatterns.FullPage, StringComparison.OrdinalIgnoreCase))
            {
                if (string.Equals(requestType, Constants.RequestType.AddPI, StringComparison.OrdinalIgnoreCase))
                {
                    return Constants.PidlResourceDescriptionType.GenericRedirectStaticPidl;
                }
                else if (string.Equals(requestType, Constants.RequestType.GetPI, StringComparison.OrdinalIgnoreCase))
                {
                    return Constants.PidlResourceDescriptionType.GenericPollingStaticPidl;
                }
            }

            return string.Empty;
        }

        private static void BuildProfileAddressDescriptionPidls(PaymentInstrument paymentInstrument, string country, string type, string language, string partner, bool primaryResource, bool overrideJarvisVersionToV3, EventTraceActivity traceActivityId, List<string> exposedFlightFeatures, PaymentExperienceSetting setting, out ClientAction clientAction, out List<PIDLResource> profileAddressPidls)
        {
            if (paymentInstrument == null)
            {
                throw TraceCore.TraceException<PXServiceException>(traceActivityId, new PXServiceException(Constants.PXServiceErrorCodes.ArgumentIsNull, "paymentInstrument"));
            }

            clientAction = new ClientAction(ClientActionType.Pidl);
            profileAddressPidls = PIDLResourceFactory.Instance.GetAddressDescriptions(country, type, language, partner, overrideJarvisVersionToV3: overrideJarvisVersionToV3, exposedFlightFeatures: exposedFlightFeatures, setting: setting);
            if (!primaryResource)
            {
                profileAddressPidls.ForEach(r => r.MakeSecondaryResource());

                if (exposedFlightFeatures != null && exposedFlightFeatures.Contains(Flighting.Features.PXAddSecondaryClientActionWithIgnoreTerminatingErrorHandling, StringComparer.OrdinalIgnoreCase))
                {
                    profileAddressPidls.ForEach(r => r.SetErrorHandlingToIgnore());
                }
            }

            PIDLResourceFactory.UpdateCancelAddressContextAfterPIAdded(profileAddressPidls, paymentInstrument.PaymentInstrumentId, partner);
        }

        private static void AddClientActionToIndiaCCRequest(PaymentInstrument paymentInstrument, string language, string partner, string classicProduct, bool completePrerequisites, string country, string scenario, string sessionQueryUrl, string requestType, string billableAccountId, string emailAddress, List<string> exposedFlightFeatures, string sessionId, string pidlBaseUrl, PaymentExperienceSetting setting = null)
        {
            PaymentInstrumentDetails paymentInstrumentDetails = paymentInstrument.PaymentInstrumentDetails;

            ClientAction clientAction;

            if (string.Equals(scenario, Constants.ScenarioNames.AzureIbiza, StringComparison.InvariantCultureIgnoreCase))
            {
                clientAction = new ClientAction(ClientActionType.Pidl);

                // In the case of ADD PI, use the sessionQueryUrl from the payment instrument response of POST PI from PIMS.  In the case of GET PI, use the sessionQueryUrl from query param.
                if (string.Equals(requestType, Constants.RequestType.AddPI, StringComparison.OrdinalIgnoreCase))
                {
                    clientAction.Context = PIDLResourceFactory.Instance.GetCc3DSRedirectAndStatusCheckDescriptionForPI(paymentInstrument, language, partner, scenario, classicProduct, completePrerequisites, country);
                }
                else if (string.Equals(requestType, Constants.RequestType.GetPI, StringComparison.OrdinalIgnoreCase))
                {
                    clientAction.Context = PIDLResourceFactory.Instance.GetCc3DSStatusCheckDescriptionForPI(paymentInstrument, language, partner, scenario, classicProduct, completePrerequisites, country, sessionQueryUrl);
                }

                paymentInstrument.ClientAction = clientAction;
                return;
            }
            else if (!string.IsNullOrEmpty(paymentInstrumentDetails.RedirectUrl) || !string.IsNullOrEmpty(sessionQueryUrl))
            {
                RedirectionServiceLink redirectLink = GetRedirectionServiceLink(paymentInstrument);
                clientAction = new ClientAction(ClientActionType.Redirect);
                clientAction.Context = redirectLink;

                // A PIDL client action is needed for one of the following scenarios:
                // 1) For any non-inline partners (redirection in new tab, need to show PIDL forms in the original page)
                // 2) For inline partners who desire to show additional page before redirecting the user, like cart partner
                // 3) For partners who have not done work to open in new tab or full page redirect, we will force iframe experience to show a PIDL form rendering iframe in it
                if (!Constants.InlinePartners.Contains(partner.ToLowerInvariant()) || PartnerHelper.IsIndiaThreeDSAddPIRedirectionInNewPagePartner(partner) || PartnerHelper.IsThreeDSOneIframeBasedPartner(partner))
                {
                    clientAction = new ClientAction(ClientActionType.Pidl);

                    // In the case of ADD PI, use the sessionQueryUrl from the payment instrument response of POST PI from PIMS.  In the case of GET PI, use the sessionQueryUrl from query param.
                    if (string.Equals(requestType, Constants.RequestType.AddPI, StringComparison.OrdinalIgnoreCase))
                    {
                        if (string.Equals(partner, Constants.PartnerName.Xbox, StringComparison.OrdinalIgnoreCase) || string.Equals(partner, Constants.PartnerName.AmcXbox) || PXCommon.Constants.PartnerGroups.IsXboxNativePartner(partner))
                        {
                            clientAction.Context = PIDLResourceFactory.Instance.GetQrCodeChallengeDescriptionForPI(paymentInstrument, language, Constants.ChallengeDescriptionTypes.ThreeDSOneQrCode, partner, classicProduct, billableAccountId, emailAddress, completePrerequisites, country, exposedFlightFeatures, sessionId, scenario);
                        }
                        else
                        {
                            if (paymentInstrument.PaymentInstrumentDetails.IsFullPageRedirect != null && paymentInstrument.PaymentInstrumentDetails.IsFullPageRedirect == false)
                            {
                                clientAction.Context = PIDLResourceFactory.GetCc3DSIframeRedirectAndStatusCheckDescriptionForPI(paymentInstrument, language, partner, scenario, classicProduct, completePrerequisites, country, pidlBaseUrl);
                            }
                            else
                            {
                                clientAction.Context = PIDLResourceFactory.Instance.GetCc3DSRedirectAndStatusCheckDescriptionForPI(paymentInstrument, language, partner, scenario, classicProduct, completePrerequisites, country, true, setting: setting);
                            }
                        }
                    }
                    else if (string.Equals(requestType, Constants.RequestType.GetPI, StringComparison.OrdinalIgnoreCase))
                    {
                        clientAction.Context = PIDLResourceFactory.Instance.GetCc3DSStatusCheckDescriptionForPI(paymentInstrument, language, partner, scenario, classicProduct, completePrerequisites, country, sessionQueryUrl, setting: setting);
                    }
                }

                paymentInstrument.ClientAction = clientAction;
                return;
            }
            else
            {
                // TODO: we might need to throw an exception here but as per the comments in the 'AddClientActionToPaymentInstrument' function
                // Once PIMS fixes the contract add pendingOn check and throw exception in the else condition (Bug 1607346)
                return;
            }
        }

        private static bool IsCreditCard(PaymentInstrument paymentInstrument)
        {
            string family = paymentInstrument.PaymentMethod.PaymentMethodFamily;
            string type = paymentInstrument.PaymentMethod.PaymentMethodType;
            bool isCardFamily = string.Equals(family, Constants.PaymentMethodFamily.credit_card.ToString(), StringComparison.OrdinalIgnoreCase);
            bool isCreditAmericanExpressType = string.Equals(type, Constants.PaymentMethodType.CreditCardAmericanExpress.ToString(), StringComparison.OrdinalIgnoreCase);
            bool isCreditDiscoverType = string.Equals(type, Constants.PaymentMethodType.CreditCardDiscover.ToString(), StringComparison.OrdinalIgnoreCase);
            bool isCreditVisaType = string.Equals(type, Constants.PaymentMethodType.CreditCardVisa.ToString(), StringComparison.OrdinalIgnoreCase);
            bool isCreditMasterCardType = string.Equals(type, Constants.PaymentMethodType.CreditCardMasterCard.ToString(), StringComparison.OrdinalIgnoreCase);
            bool isCreditRupayType = string.Equals(type, Constants.PaymentMethodType.CreditCardRupay.ToString(), StringComparison.OrdinalIgnoreCase);
            return isCardFamily && (isCreditAmericanExpressType || isCreditDiscoverType || isCreditVisaType || isCreditMasterCardType || isCreditRupayType);
        }

        private static bool IsKakaopay(PaymentInstrument paymentInstrument)
        {
            if (string.Equals(paymentInstrument.PaymentMethod.PaymentMethodType, Constants.PaymentMethodType.Kakaopay, StringComparison.InvariantCultureIgnoreCase))
            {
                return true;
            }

            return false;
        }

        private static bool IsIdealBillingAgreement(PaymentInstrument paymentInstrument)
        {
            if (string.Equals(paymentInstrument.PaymentMethod.PaymentMethodType, Constants.PaymentMethodType.IdealBillingAgreement, StringComparison.InvariantCultureIgnoreCase))
            {
                return true;
            }

            return false;
        }

        private static bool IsGenericRedirect(PaymentInstrument paymentInstrument)
        {
            if (string.Equals(paymentInstrument.PaymentMethod.PaymentMethodType, Constants.PaymentMethodType.Kakaopay, StringComparison.InvariantCultureIgnoreCase)
                || string.Equals(paymentInstrument.PaymentMethod.PaymentMethodType, Constants.PaymentMethodType.Klarna, StringComparison.InvariantCultureIgnoreCase)
                || paymentInstrument.IsPaymentMethodType(Constants.PaymentMethodFamily.ewallet, Constants.PaymentMethodType.PayPay)
                || paymentInstrument.IsPaymentMethodType(Constants.PaymentMethodFamily.ewallet, Constants.PaymentMethodType.AlipayHK)
                || paymentInstrument.IsPaymentMethodType(Constants.PaymentMethodFamily.ewallet, Constants.PaymentMethodType.GCash)
                || paymentInstrument.IsPaymentMethodType(Constants.PaymentMethodFamily.ewallet, Constants.PaymentMethodType.TrueMoney)
                || paymentInstrument.IsPaymentMethodType(Constants.PaymentMethodFamily.ewallet, Constants.PaymentMethodType.TouchNGo)
                || paymentInstrument.IsPaymentMethodType(Constants.PaymentMethodFamily.ewallet, Constants.PaymentMethodType.AlipayCN))
            {
                return true;
            }

            return false;
        }

        private static bool IsInvoiceCreditKlarna(PaymentInstrument paymentInstrument)
        {
            return string.Equals(paymentInstrument.PaymentMethod.PaymentMethodFamily, Constants.PaymentMethodFamily.invoice_credit.ToString(), StringComparison.OrdinalIgnoreCase)
                && string.Equals(paymentInstrument.PaymentMethod.PaymentMethodType, Constants.PaymentMethodType.Klarna, StringComparison.OrdinalIgnoreCase);
        }

        private static bool DoesSupportGenericQrCode(PaymentInstrument paymentInstrument)
        {
            if (string.Equals(paymentInstrument.PaymentMethod.PaymentMethodType, Constants.PaymentMethodType.Klarna, StringComparison.InvariantCultureIgnoreCase))
            {
                return true;
            }

            return false;
        }

        private static bool IsAch(PaymentInstrument paymentInstrument)
        {
            if (string.Equals(paymentInstrument.PaymentMethod.PaymentMethodType, Constants.PaymentMethodType.Ach, StringComparison.InvariantCultureIgnoreCase))
            {
                return true;
            }

            return false;
        }

        private static bool IsSepa(PaymentInstrument paymentInstrument)
        {
            if (string.Equals(paymentInstrument.PaymentMethod.PaymentMethodType, Constants.PaymentMethodType.Sepa, StringComparison.InvariantCultureIgnoreCase))
            {
                return true;
            }

            return false;
        }

        private static bool IsNonSimMobi(PaymentInstrument paymentInstrument)
        {
            if (string.Equals(paymentInstrument.PaymentMethod.PaymentMethodFamily, Constants.PaymentMethodFamily.mobile_billing_non_sim.ToString(), StringComparison.InvariantCultureIgnoreCase))
            {
                return true;
            }

            return false;
        }

        private static bool IsAlipay(PaymentInstrument paymentInstrument)
        {
            if (string.Equals(paymentInstrument.PaymentMethod.PaymentMethodType, Constants.PaymentMethodType.AlipayBillingAgreement, StringComparison.InvariantCultureIgnoreCase))
            {
                return true;
            }

            return false;
        }

        private static bool IsChinaUnionPay(PaymentInstrument paymentInstrument)
        {
            if (string.Equals(paymentInstrument.PaymentMethod.PaymentMethodType, Constants.PaymentMethodType.UnionPayCreditCard, StringComparison.InvariantCultureIgnoreCase))
            {
                return true;
            }

            if (string.Equals(paymentInstrument.PaymentMethod.PaymentMethodType, Constants.PaymentMethodType.UnionPayDebitCard, StringComparison.InvariantCultureIgnoreCase))
            {
                return true;
            }

            return false;
        }

        private static void AddClientActionToKakaopayRequest(PaymentInstrument paymentInstrument, string language, string partner, string requestType, bool completePrerequisites, string country, string scenario, string emailAddress, List<string> exposedFlightFeatures = null, string sessionId = null, string shortUrl = null)
        {
            PaymentInstrumentDetails paymentInstrumentDetails = paymentInstrument.PaymentInstrumentDetails;

            if ((string.Equals(requestType, Constants.RequestType.AddPI, StringComparison.OrdinalIgnoreCase) || string.Equals(requestType, Constants.RequestType.GetPI, StringComparison.OrdinalIgnoreCase))
                && !string.IsNullOrEmpty(paymentInstrumentDetails.RedirectUrl))
            {
                ClientAction clientAction = new ClientAction(ClientActionType.Pidl);
                clientAction.Context = PIDLResourceFactory.Instance.GetKakaopayChallengeDescriptionForPI(paymentInstrument, language, Constants.ChallengeDescriptionTypes.KakaopayQrCode, partner, null, null, null, completePrerequisites, country);
                paymentInstrument.ClientAction = clientAction;
                return;
            }
        }

        private static void AddClientActionToGenericRedirectRequest(PaymentInstrument paymentInstrument, string language, string partner, string requestType, bool completePrerequisites, string country, string scenario)
        {
            PaymentInstrumentDetails paymentInstrumentDetails = paymentInstrument.PaymentInstrumentDetails;

            if (string.Equals(requestType, Constants.RequestType.AddPI, StringComparison.OrdinalIgnoreCase) && !string.IsNullOrEmpty(paymentInstrumentDetails.RedirectUrl))
            {
                if (string.Equals(scenario, Constants.ScenarioNames.GenericQrCode, StringComparison.OrdinalIgnoreCase) && DoesSupportGenericQrCode(paymentInstrument))
                {
                    ClientAction clientAction = new ClientAction(ClientActionType.Pidl);
                    clientAction.Context = PIDLResourceFactory.Instance.GetQrCodeChallengeDescriptionForPI(paymentInstrument, language, Constants.ChallengeDescriptionTypes.GenericQrCode, partner, null, null, null, completePrerequisites, country);
                    paymentInstrument.ClientAction = clientAction;
                    return;
                }
                else
                {
                    ClientAction clientAction = new ClientAction(ClientActionType.Redirect);
                    clientAction.Context = paymentInstrument.PaymentInstrumentDetails.RedirectUrl;

                    if (paymentInstrument.IsPaymentMethodType(Constants.PaymentMethodFamily.ewallet, Constants.PaymentMethodType.PayPay)
                        || paymentInstrument.IsPaymentMethodType(Constants.PaymentMethodFamily.ewallet, Constants.PaymentMethodType.AlipayHK)
                        || paymentInstrument.IsPaymentMethodType(Constants.PaymentMethodFamily.ewallet, Constants.PaymentMethodType.GCash)
                        || paymentInstrument.IsPaymentMethodType(Constants.PaymentMethodFamily.ewallet, Constants.PaymentMethodType.TrueMoney)
                        || paymentInstrument.IsPaymentMethodType(Constants.PaymentMethodFamily.ewallet, Constants.PaymentMethodType.TouchNGo)
                        || paymentInstrument.IsPaymentMethodType(Constants.PaymentMethodFamily.ewallet, Constants.PaymentMethodType.AlipayCN))
                    {
                        RedirectionServiceLink redirectLink = GetRedirectionServiceLink(paymentInstrument);
                        clientAction.Context = redirectLink;
                    }

                    if (!Constants.InlinePartners.Contains(partner.ToLowerInvariant())
                        || string.Equals(paymentInstrument.PaymentMethod.PaymentMethodType, Constants.PaymentMethodType.Klarna, StringComparison.InvariantCultureIgnoreCase))
                    {
                        clientAction.RedirectPidl = PIDLResourceFactory.Instance.GetRedirectPidlForPI(paymentInstrument, Constants.PidlResourceDescriptionType.GenericRedirectStaticPidl, language, partner, completePrerequisites, country);
                    }

                    paymentInstrument.ClientAction = clientAction;
                    return;
                }
            }
            else if (string.Equals(requestType, Constants.RequestType.GetPI, StringComparison.OrdinalIgnoreCase))
            {
                ClientAction clientAction = new ClientAction(ClientActionType.Pidl);
                clientAction.Context = PIDLResourceFactory.Instance.GetGenericPollPidlDescriptions(
                    type: Constants.PidlResourceDescriptionType.GenericPollingStaticPidl,
                    language: language,
                    partnerName: partner,
                    paymentInstrument: paymentInstrument,
                    completePrerequisites: completePrerequisites,
                    country: country);

                paymentInstrument.ClientAction = clientAction;
                return;
            }
        }

        private static void AddClientActionToPaypalRequest(PaymentInstrument paymentInstrument, string language, string partner, string requestType, bool completePrerequisites, string country, string scenario, string emailAddress, List<string> exposedFlightFeatures = null, string sessionId = null, string shortUrl = null, PaymentExperienceSetting setting = null)
        {
            PaymentInstrumentDetails paymentInstrumentDetails = paymentInstrument.PaymentInstrumentDetails;

            if (!string.IsNullOrEmpty(requestType) && string.Equals(requestType, Constants.RequestType.GetPI, StringComparison.OrdinalIgnoreCase))
            {
                ClientAction clientAction = new ClientAction(ClientActionType.Pidl);
                if (!string.IsNullOrEmpty(paymentInstrumentDetails.PendingOn) && string.Equals(paymentInstrumentDetails.PendingOn, Constants.PaymentInstrumentPendingOnTypes.AgreementUpdate, StringComparison.OrdinalIgnoreCase))
                {
                    clientAction.Context = PIDLResourceFactory.Instance.GetUpdateAgreementChallengeDescriptionForPI(paymentInstrument, Constants.PidlResourceDescriptionType.PaypalUpdateAgreementChallenge, language, partner);
                }
                else
                {
                    clientAction.Context = PIDLResourceFactory.Instance.GetStaticPidlDescriptions(Constants.PidlResourceDescriptionType.PaypalRetryStaticPidl, language, partner, setting: setting);
                }

                paymentInstrument.ClientAction = clientAction;
                return;
            }
            else if (!string.IsNullOrEmpty(paymentInstrumentDetails.RedirectUrl))
            {
                if (PIDLResourceFactory.IsTemplateInList(partner, setting, Constants.DescriptionTypes.PaymentMethodDescription, $"{paymentInstrument.PaymentMethod.PaymentMethodFamily}.{paymentInstrument.PaymentMethod.PaymentMethodType}") || string.Equals(scenario, Constants.ScenarioNames.PaypalQrCode, StringComparison.OrdinalIgnoreCase))
                {
                    ClientAction clientAction = new ClientAction(ClientActionType.Pidl);
                    clientAction.Context = PIDLResourceFactory.Instance.GetQrCodeChallengeDescriptionForPI(paymentInstrument, language, Constants.ChallengeDescriptionTypes.PaypalQrCode, partner, null, null, emailAddress, completePrerequisites, country, exposedFlightFeatures, sessionId, scenario, shortUrl: shortUrl?.ToString(), setting: setting);
                    paymentInstrument.ClientAction = clientAction;
                    return;
                }
                else
                {
                    RedirectionServiceLink redirectLink = GetRedirectionServiceLink(paymentInstrument);
                    ClientAction clientAction = new ClientAction(ClientActionType.Redirect);
                    clientAction.Context = redirectLink;

                    if (!Constants.InlinePartners.Contains(partner.ToLowerInvariant()))
                    {
                        clientAction.RedirectPidl = PIDLResourceFactory.Instance.GetRedirectPidlForPI(paymentInstrument, Constants.PidlResourceDescriptionType.PaypalRedirectStaticPidl, language, partner, completePrerequisites, country, flightNames: exposedFlightFeatures, setting: setting);
                    }

                    paymentInstrument.ClientAction = clientAction;
                    return;
                }
            }
            else
            {
                // TODO: we might need to throw an exception here but as per the comments in the 'AddClientActionToPaymentInstrument' function
                // Once PIMS fixes the contract add pendingOn check and throw exception in the else condition (Bug 1607346)
                return;
            }
        }

        private static void AddClientActionToVenmoRequest(PaymentInstrument paymentInstrument, string language, string partner, string requestType, bool completePrerequisites, string country, string scenario, string emailAddress, List<string> exposedFlightFeatures = null, string sessionId = null, string shortUrl = null, PaymentExperienceSetting setting = null)
        {
            if (exposedFlightFeatures == null)
            {
                exposedFlightFeatures = new List<string>();
            }

            PaymentInstrumentDetails paymentInstrumentDetails = paymentInstrument.PaymentInstrumentDetails;

            if (!string.IsNullOrEmpty(requestType) && string.Equals(requestType, Constants.RequestType.GetPI, StringComparison.OrdinalIgnoreCase) && !PXCommon.Constants.PartnerGroups.IsXboxNativePartner(partner))
            {
                paymentInstrument.ClientAction = new ClientAction(ClientActionType.Pidl)
                {
                    Context = PIDLResourceFactory.Instance.GetStaticPidlDescriptions(Constants.PidlResourceDescriptionType.VenmoRetryStaticPidl, language, partner, setting: setting)
                };

                return;
            }
            else if (!string.IsNullOrEmpty(paymentInstrumentDetails.RedirectUrl))
            {
                if (string.Equals(scenario, Constants.ScenarioNames.VenmoQRCode, StringComparison.OrdinalIgnoreCase))
                {
                    ClientAction clientAction = new ClientAction(ClientActionType.Pidl);
                    clientAction.Context = PIDLResourceFactory.Instance.GetQrCodeChallengeDescriptionForPI(paymentInstrument, language, Constants.ChallengeDescriptionTypes.VenmoQrCode, partner, null, null, emailAddress, completePrerequisites, country, exposedFlightFeatures, sessionId, scenario, shortUrl: shortUrl?.ToString(), setting: setting);
                    paymentInstrument.ClientAction = clientAction;

                    return;
                }
                else
                {
                    ClientAction clientAction = new ClientAction(ClientActionType.Redirect);
                    clientAction.Context = GetRedirectionServiceLink(paymentInstrument);

                    if (!Constants.InlinePartners.Contains(partner.ToLowerInvariant()))
                    {
                        clientAction.RedirectPidl = PIDLResourceFactory.Instance.GetRedirectPidlForPI(paymentInstrument, Constants.PidlResourceDescriptionType.VenmoRedirectStaticPidl, language, partner, completePrerequisites, country, flightNames: exposedFlightFeatures, setting: setting, sessionId: sessionId);
                    }

                    paymentInstrument.ClientAction = clientAction;

                    return;
                }
            }
            else
            {
                // TODO: we might need to throw an exception here but as per the comments in the 'AddClientActionToPaymentInstrument' function
                // Once PIMS fixes the contract add pendingOn check and throw exception in the else condition (Bug 1607346)
                return;
            }
        }

        private static void AddClientActionToAchRequest(PaymentInstrument paymentInstrument, string language, string partner, string classicProduct, string billableAccountId, EventTraceActivity traceActivityId, string requestType)
        {
            PaymentInstrumentDetails paymentInstrumentDetails = paymentInstrument.PaymentInstrumentDetails;

            if (!string.IsNullOrEmpty(paymentInstrumentDetails.PendingOn))
            {
                if (string.Equals(paymentInstrumentDetails.PendingOn, Constants.PaymentInstrumentPendingOnTypes.Picv, StringComparison.OrdinalIgnoreCase))
                {
                    // For ACH, if it comes from an addPI operation with pendingon equals to picv, show the static info page.
                    // If it comes from a getPI operation with pendingon equals to picv, show the challenge page.
                    // Else, throw exception
                    if (!string.IsNullOrEmpty(requestType) && string.Equals(requestType, Constants.RequestType.AddPI, StringComparison.OrdinalIgnoreCase))
                    {
                        // TODO: Task 55458563, PIMS has already deprecated the PIDL for Add ACH. Therefore, for the template partner, we can avoid mirgrating the Add ACH flow.
                        if (!partner.Equals(Constants.PartnerName.Cart, StringComparison.OrdinalIgnoreCase))
                        {
                            ClientAction clientAction = new ClientAction(ClientActionType.Pidl);
                            clientAction.Context = PIDLResourceFactory.Instance.GetStaticPidlDescriptions(Constants.PidlResourceDescriptionType.AchPicVStatic, language, partner);
                            paymentInstrument.ClientAction = clientAction;
                        }

                        return;
                    }
                    else if (!string.IsNullOrEmpty(requestType) && string.Equals(requestType, Constants.RequestType.GetPI, StringComparison.OrdinalIgnoreCase))
                    {
                        ClientAction clientAction = new ClientAction(ClientActionType.Pidl);
                        clientAction.Context = PIDLResourceFactory.Instance.GetPicvChallengeDescriptionForPI(paymentInstrument, Constants.PidlResourceDescriptionType.AchPicVChallenge, language, partner, classicProduct, billableAccountId);
                        paymentInstrument.ClientAction = clientAction;
                        return;
                    }
                    else
                    {
                        throw TraceCore.TraceException<IntegrationException>(
                            traceActivityId,
                            new IntegrationException(
                                PXCommonConstants.ServiceNames.InstrumentManagementService,
                                string.Format("The operation type of PI was expected to be addPI or getPI. Actual operation type {0}", requestType),
                                Constants.PXServiceIntegrationErrorCodes.InvalidPendingOnType));
                    }
                }
                else
                {
                    throw TraceCore.TraceException<IntegrationException>(
                        traceActivityId,
                        new IntegrationException(
                            PXCommonConstants.ServiceNames.InstrumentManagementService,
                            string.Format("The state of PI was expected to be pending on PICV. Actual state {0}", paymentInstrumentDetails.PendingOn),
                            Constants.PXServiceIntegrationErrorCodes.InvalidPendingOnType));
                }
            }
        }

        private static void AddClientActionToSepaRequest(PaymentInstrument paymentInstrument, string language, string partner, string classicProduct, string billableAccountId, EventTraceActivity traceActivityId, List<string> exposedFlightFeatures = null, PaymentExperienceSetting setting = null, bool completePrerequisites = false, string country = null, string scenario = null)
        {
            PaymentInstrumentDetails paymentInstrumentDetails = paymentInstrument.PaymentInstrumentDetails;

            if (string.Equals(paymentInstrument.Status.ToString(), Constants.PaymentInstrumentStatus.Pending, StringComparison.OrdinalIgnoreCase))
            {
                // For SEPA, if its pendingon equals redirect, show the static info page.
                // Else, pass through
                if (string.Equals(paymentInstrumentDetails.PendingOn, Constants.PaymentInstrumentPendingOnTypes.Redirect, StringComparison.OrdinalIgnoreCase))
                {
                    if (!string.IsNullOrEmpty(paymentInstrumentDetails.RedirectUrl))
                    {
                        RedirectionServiceLink redirectLink = GetRedirectionServiceLink(paymentInstrument);
                        ClientAction clientAction = new ClientAction(ClientActionType.Redirect);

                        if (PartnerSettingsHelper.IsFeatureEnabledUsingPartnerSettings(PartnerSettingsHelper.Features.UseTwoStaticPageRedirection, null, setting))
                        {
                            clientAction = new ClientAction(ClientActionType.Pidl);
                            clientAction.Context = PIDLResourceFactory.Instance.GetSepaRedirectAndStatusCheckDescriptionForPI(paymentInstrument, language, partner, scenario, classicProduct, completePrerequisites, country, enablePolling: false, setting: setting);
                            paymentInstrument.ClientAction = clientAction;
                            return;
                        }

                        clientAction.Context = redirectLink;

                        if (!Constants.InlinePartners.Contains(partner.ToLowerInvariant()))
                        {
                            clientAction.RedirectPidl = PIDLResourceFactory.Instance.GetStaticPidlDescriptions(Constants.PidlResourceDescriptionType.SepaPicVStatic, language, partner, redirectLink, paymentInstrument, flightNames: exposedFlightFeatures);
                        }

                        paymentInstrument.ClientAction = clientAction;
                        return;
                    }
                }
            }
            else if (string.Equals(paymentInstrument.Status.ToString(), Constants.PaymentInstrumentStatus.Active, StringComparison.OrdinalIgnoreCase) && !string.Equals(partner, Constants.PartnerName.Azure, StringComparison.OrdinalIgnoreCase) && !string.Equals(partner, Constants.PartnerName.AzureManage, StringComparison.OrdinalIgnoreCase))
            {
                // Currently for the new integration of SEPA redirection, static pages are being implemented for Azure and AzureManage only.
                // In this flow, GET PI call happens with active PI status which triggers this flow, hence excluding these two storefronts.
                // Task 20657986: [PM Task] PIDL static page for "failed", "expired" and "0 retry time" SEPA PI
                // PxService might need to show static page for failed, expired PicvStatus based on the above PM task
                // RemainingAttempts is a valid number that is greater than 0
                if (paymentInstrumentDetails.PicvDetails != null && string.Equals(paymentInstrumentDetails.PicvDetails.Status, Constants.PicvStatus.InProgress, StringComparison.OrdinalIgnoreCase))
                {
                    if (paymentInstrumentDetails.PicvDetails.RemainingAttempts == null)
                    {
                        throw TraceCore.TraceException<IntegrationException>(
                            traceActivityId,
                            new IntegrationException(
                                PXCommonConstants.ServiceNames.InstrumentManagementService,
                                string.Format("There must be a non-empty 'remainingAttemps' property for 'inProgress' SEPA PI"),
                                Constants.PXServiceIntegrationErrorCodes.InvalidPicvDetailsPayload));
                    }

                    ClientAction clientAction = new ClientAction(ClientActionType.Pidl);
                    clientAction.Context = PIDLResourceFactory.Instance.GetPicvChallengeDescriptionForPI(paymentInstrument, Constants.PidlResourceDescriptionType.SepaPicVChallenge, language, partner, classicProduct, billableAccountId);
                    paymentInstrument.ClientAction = clientAction;
                    return;
                }
            }
        }

        private static RedirectionServiceLink GetRedirectionServiceLink(PaymentInstrument paymentInstrument)
        {
            PaymentInstrumentDetails paymentInstrumentDetails = paymentInstrument.PaymentInstrumentDetails;
            RedirectionServiceLink redirectLink = new RedirectionServiceLink { BaseUrl = paymentInstrumentDetails.RedirectUrl };
            redirectLink.RuParameters.Add("id", paymentInstrument.PaymentInstrumentId);
            redirectLink.RuParameters.Add("family", paymentInstrument.PaymentMethod.PaymentMethodFamily);
            redirectLink.RuParameters.Add("type", paymentInstrument.PaymentMethod.PaymentMethodType);
            redirectLink.RuParameters.Add("pendingOn", paymentInstrumentDetails.PendingOn);
            redirectLink.RuParameters.Add("picvRequired", paymentInstrumentDetails.PicvRequired.ToString());

            return redirectLink;
        }

        private static bool IsSupportedPSSRedirectPI(PaymentInstrument paymentInstrument, string country)
        {
            return paymentInstrument.IsVenmo()
                || paymentInstrument.IsPayPal()
                || IsInvoiceCreditKlarna(paymentInstrument)
                || IsKakaopay(paymentInstrument)
                || (IsCreditCard(paymentInstrument) && string.Equals(country, "in", StringComparison.OrdinalIgnoreCase))
                || paymentInstrument.IsPaymentMethodType(Constants.PaymentMethodFamily.ewallet, Constants.PaymentMethodType.PayPay)
                || paymentInstrument.IsPaymentMethodType(Constants.PaymentMethodFamily.ewallet, Constants.PaymentMethodType.AlipayHK)
                || paymentInstrument.IsPaymentMethodType(Constants.PaymentMethodFamily.ewallet, Constants.PaymentMethodType.GCash)
                || paymentInstrument.IsPaymentMethodType(Constants.PaymentMethodFamily.ewallet, Constants.PaymentMethodType.TrueMoney)
                || paymentInstrument.IsPaymentMethodType(Constants.PaymentMethodFamily.ewallet, Constants.PaymentMethodType.TouchNGo)
                || paymentInstrument.IsPaymentMethodType(Constants.PaymentMethodFamily.ewallet, Constants.PaymentMethodType.AlipayCN)
                || IsSepa(paymentInstrument);
        }
    }
}