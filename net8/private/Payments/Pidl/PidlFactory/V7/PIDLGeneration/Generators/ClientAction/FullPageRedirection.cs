// <copyright file="FullPageRedirection.cs" company="Microsoft">Copyright (c) Microsoft 2023. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PidlFactory.V7.PIDLGeneration.Generators
{
    using System;
    using Microsoft.Commerce.Payments.Common.Tracing;
    using Microsoft.Commerce.Payments.PimsModel.V4;
    using Microsoft.Commerce.Payments.PXCommon;

    internal class FullPageRedirection : IPIDLGenerator<ClientAction>
    {
        public ClientAction Generate(PIDLGeneratorContext context)
        {
            if (string.Equals(context.OperationType, V7.Constants.PidlOperationTypes.HandlePaymentChallenge, StringComparison.OrdinalIgnoreCase))
            {
                return GetClientActionForHandlePaymentChallenge(context);
            }

            if (string.Equals(context.OperationType, V7.Constants.PidlOperationTypes.Add, StringComparison.OrdinalIgnoreCase))
            {
                return GetClientActionForAddPI(context);
            }

            return null;
        }

        private static ClientAction GetClientActionForAddPI(PIDLGeneratorContext context)
        {
            ClientAction clientAction = new ClientAction(ClientActionType.Pidl);

            if (string.Equals(context.PaymentInstrument.PaymentMethod.PaymentMethodFamily, V7.Constants.PaymentMethodFamilyNames.CreditCard, StringComparison.OrdinalIgnoreCase) && string.Equals(context.Country, "in", StringComparison.OrdinalIgnoreCase))
            {
                return GetClientActionForIndia3DSAddPI(context);
            }

            if (string.Equals(context.RequestType, V7.Constants.RequestType.GetPI, StringComparison.OrdinalIgnoreCase))
            {
                if (string.Equals(context.DescriptionType, Constants.StaticDescriptionTypes.GenericPollingStaticPidl))
                {
                    clientAction = new ClientAction(ClientActionType.Pidl);
                    clientAction.Context = PIDLResourceFactory.Instance.GetGenericPollPidlDescriptions(
                        type: context.DescriptionType,
                        language: context.Language,
                        partnerName: context.OriginalPartner ?? context.Partner,
                        paymentInstrument: context.PaymentInstrument,
                        completePrerequisites: context.CompletePrerequisites,
                        country: context.Country,
                        setting: context.PartnerSetting);

                    return clientAction;
                }

                string redirectUrl = context.PaymentInstrument.PaymentInstrumentDetails.RedirectUrl;
                clientAction.Context = PIDLResourceFactory.Instance.GetStaticPidlDescriptions(
                   context.DescriptionType,
                   context.Language,
                   context.OriginalPartner ?? context.Partner,
                   !string.IsNullOrEmpty(redirectUrl) ? GetRedirectionServiceLink(context.PaymentInstrument) : null,
                   context.PaymentInstrument,
                   context.ExposedFlightFeatures,
                   context.PartnerSetting);
            }

            if (string.Equals(context.RequestType, V7.Constants.RequestType.AddPI, StringComparison.OrdinalIgnoreCase))
            {
                if (PIDLResourceFactory.ShouldGeneratePidlClientActionForAddPIPicv(context))
                {
                    if (context.PaymentInstrument.PaymentInstrumentDetails.PicvDetails.RemainingAttempts == null)
                    {
                        throw TraceCore.TraceException<IntegrationException>(
                            context.TraceActivityId,
                            new IntegrationException(
                                Constants.ServiceNames.InstrumentManagementService,
                                string.Format("There must be a non-empty 'remainingAttemps' property for 'inProgress' SEPA PI"),
                                Constants.PIDLIntegrationErrorCodes.InvalidPicvDetailsPayload.ToString()));
                    }

                    clientAction = new ClientAction(ClientActionType.Pidl);
                    clientAction.Context = PIDLResourceFactory.Instance.GetPicvChallengeDescriptionForPI(
                        context.PaymentInstrument,
                        context.DescriptionType,
                        context.Language,
                        partnerName: context.OriginalPartner ?? context.Partner,
                        context.ClassicProduct,
                        context.BillableAccountId,
                        context.PartnerSetting);
                }
                else
                {
                    clientAction = new ClientAction(ClientActionType.Redirect);

                    if (IsVenmo(context.PaymentInstrument)
                        || IsPayPal(context.PaymentInstrument) 
                        || IsANTPIs(context.PaymentInstrument)
                        || IsAlipayCN(context.PaymentInstrument)
                        || IsSepa(context.PaymentInstrument))
                    {
                        clientAction.Context = GetRedirectionServiceLink(context.PaymentInstrument);
                    }
                    else
                    {
                        clientAction.Context = context.PaymentInstrument.PaymentInstrumentDetails.RedirectUrl;
                    }

                    string redirectUrl = context.PaymentInstrument.PaymentInstrumentDetails.RedirectUrl;

                    clientAction.RedirectPidl = PIDLResourceFactory.Instance.GetRedirectPidlForPI(
                        context.PaymentInstrument,
                        context.DescriptionType,
                        context.Language,
                        partnerName: context.OriginalPartner ?? context.Partner,
                        context.CompletePrerequisites,
                        context.Country,
                        context.ExposedFlightFeatures,
                        context.PartnerSetting,
                        context.SessionId,
                        redirectLink: !string.IsNullOrEmpty(redirectUrl) ? GetRedirectionServiceLink(context.PaymentInstrument) : null);
                }
            }

            return clientAction;
        }

        private static ClientAction GetClientActionForIndia3DSAddPI(PIDLGeneratorContext context)
        {
            ClientAction clientAction = new ClientAction(ClientActionType.Pidl);

            if (string.Equals(context.RequestType, V7.Constants.RequestType.GetPI, StringComparison.OrdinalIgnoreCase))
            {
                clientAction.Context = PIDLResourceFactory.Instance.GetCc3DSStatusCheckDescriptionForPI(
                    context.PaymentInstrument,
                    context.Language,
                    context.OriginalPartner ?? context.Partner,
                    context.Scenario,
                    context.ClassicProduct,
                    context.CompletePrerequisites,
                    context.Country,
                    context.SessionQueryUrl,
                    setting: context.PartnerSetting);
            }

            if (string.Equals(context.RequestType, V7.Constants.RequestType.AddPI, StringComparison.OrdinalIgnoreCase))
            {
                clientAction.Context = PIDLResourceFactory.Instance.GetCc3DSRedirectAndStatusCheckDescriptionForPI(
                    context.PaymentInstrument,
                    context.Language,
                    context.OriginalPartner ?? context.Partner,
                    context.Scenario,
                    context.ClassicProduct,
                    context.CompletePrerequisites,
                    context.Country,
                    true,
                    setting: context.PartnerSetting);
            }

            return clientAction;
        }

        private static ClientAction GetClientActionForHandlePaymentChallenge(PIDLGeneratorContext context)
        {
            ClientAction clientAction = new ClientAction(ClientActionType.Pidl);

            if (context.UseTransactionServiceForPaymentAuth)
            {
                if (string.Equals(context.DescriptionType, Constants.StaticDescriptionTypes.Cc3DSRedirectAndStatusCheckPidl, StringComparison.OrdinalIgnoreCase)
                    || string.Equals(context.DescriptionType, Constants.StaticDescriptionTypes.LegacyBillDesk3DSRedirectAndStatusCheckPidl, StringComparison.OrdinalIgnoreCase))
                {
                    clientAction.Context = PIDLResourceFactory.Instance.Get3DSRedirectAndStatusCheckDescriptionForPaymentAuth(
                        context.RedirectUrl,
                        context.RedirectSessionId,
                        context.SessionId,
                        context.OriginalPartner ?? context.Partner,
                        context.Language,
                        context.Country,
                        context.DescriptionType,
                        context.Scenario,
                        context.PaymentMethodType,
                        setting: context.PartnerSetting);
                }

                if (string.Equals(context.DescriptionType, Constants.StaticDescriptionTypes.Cc3DSStatusCheckPidl, StringComparison.OrdinalIgnoreCase)
                    || string.Equals(context.DescriptionType, Constants.StaticDescriptionTypes.LegacyBillDesk3DSStatusCheckPidl, StringComparison.OrdinalIgnoreCase))
                {
                    clientAction.Context = PIDLResourceFactory.Instance.Get3DSStatusCheckDescriptionForPaymentAuth(
                        context.RedirectSessionId,
                        context.SessionId,
                        context.OriginalPartner ?? context.Partner,
                        context.Language,
                        context.Country,
                        context.DescriptionType,
                        context.Scenario,
                        context.PaymentMethodType,
                        setting: context.PartnerSetting);
                }
            }
            else
            {
                if (string.Equals(context.DescriptionType, Constants.StaticDescriptionTypes.Cc3DSRedirectAndStatusCheckPidl, StringComparison.OrdinalIgnoreCase))
                {
                    // Note: It is not necessary to send the OriginalPartner name because the submit link generated below does not utilize the partner name.
                    clientAction.Context = PIDLResourceFactory.Instance.GetCc3DSRedirectAndStatusCheckDescriptionForPaymentSession(
                        context.SessionId,
                        context.Language,
                        context.Partner,
                        context.RedirectUrl,
                        context.PartnerSetting);
                }

                if (string.Equals(context.DescriptionType, Constants.StaticDescriptionTypes.Cc3DSStatusCheckPidl, StringComparison.OrdinalIgnoreCase))
                {
                    // Note: It is not necessary to send the OriginalPartner name because the submit link generated below does not utilize the partner name.
                    clientAction.Context = PIDLResourceFactory.Instance.GetCc3DSStatusCheckDescriptionForPaymentSession(
                        context.SessionId,
                        context.Language,
                        context.Partner,
                        context.RedirectUrl,
                        context.PartnerSetting);
                }
            }

            return clientAction;
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

        private static bool IsPayPal(PaymentInstrument pi)
        {
            return string.Equals(pi.PaymentMethod.PaymentMethodFamily, V7.Constants.PaymentMethodFamilyNames.Ewallet, StringComparison.OrdinalIgnoreCase)
                && string.Equals(pi.PaymentMethod.PaymentMethodType, V7.Constants.PaymentMethodTypeNames.Paypal, StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsVenmo(PaymentInstrument pi)
        {
            return string.Equals(pi.PaymentMethod.PaymentMethodFamily, V7.Constants.PaymentMethodFamilyNames.Ewallet, StringComparison.OrdinalIgnoreCase)
                && string.Equals(pi.PaymentMethod.PaymentMethodType, V7.Constants.PaymentMethodTypeNames.Venmo, StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsSepa(PaymentInstrument pi)
        {
            return string.Equals(pi.PaymentMethod.PaymentMethodFamily, V7.Constants.PaymentMethodFamilyNames.DirectDebit, StringComparison.OrdinalIgnoreCase)
                && string.Equals(pi.PaymentMethod.PaymentMethodType, V7.Constants.PaymentMethodTypeNames.Sepa, StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsANTPIs(PaymentInstrument pi)
        {
            return string.Equals(pi.PaymentMethod.PaymentMethodFamily, V7.Constants.PaymentMethodFamilyNames.Ewallet, StringComparison.OrdinalIgnoreCase)
                && (string.Equals(pi.PaymentMethod.PaymentMethodType, V7.Constants.PaymentMethodTypeNames.PayPay, StringComparison.OrdinalIgnoreCase)
                    || string.Equals(pi.PaymentMethod.PaymentMethodType, V7.Constants.PaymentMethodTypeNames.AlipayHK, StringComparison.OrdinalIgnoreCase)
                    || string.Equals(pi.PaymentMethod.PaymentMethodType, V7.Constants.PaymentMethodTypeNames.GCash, StringComparison.OrdinalIgnoreCase)
                    || string.Equals(pi.PaymentMethod.PaymentMethodType, V7.Constants.PaymentMethodTypeNames.TrueMoney, StringComparison.OrdinalIgnoreCase)
                    || string.Equals(pi.PaymentMethod.PaymentMethodType, V7.Constants.PaymentMethodTypeNames.TouchNGo, StringComparison.OrdinalIgnoreCase));
        }

        private static bool IsAlipayCN(PaymentInstrument pi)
        {
            return string.Equals(pi.PaymentMethod.PaymentMethodFamily, V7.Constants.PaymentMethodFamilyNames.Ewallet, StringComparison.OrdinalIgnoreCase)
                && string.Equals(pi.PaymentMethod.PaymentMethodType, V7.Constants.PaymentMethodTypeNames.AlipayCN, StringComparison.OrdinalIgnoreCase);
        }
    }
}
