// <copyright file="InlineRedirection.cs" company="Microsoft">Copyright (c) Microsoft 2023. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PidlFactory.V7.PIDLGeneration.Generators
{
    using System;
    using Microsoft.Commerce.Payments.Common.Tracing;
    using Microsoft.Commerce.Payments.PimsModel.V4;
    using Microsoft.Commerce.Payments.PXCommon;

    internal class InlineRedirection : IPIDLGenerator<ClientAction>
    {
        public ClientAction Generate(PIDLGeneratorContext context)
        {
            if ((string.Equals(context.RequestType, V7.Constants.RequestType.AddPI, StringComparison.OrdinalIgnoreCase) || string.Equals(context.RequestType, V7.Constants.RequestType.GetPI, StringComparison.OrdinalIgnoreCase)) && PIDLResourceFactory.ShouldGeneratePidlClientActionForAddPIPicv(context))
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

                ClientAction clientAction = new ClientAction(ClientActionType.Pidl);
                clientAction.Context = PIDLResourceFactory.Instance.GetPicvChallengeDescriptionForPI(
                    context.PaymentInstrument,
                    context.DescriptionType,
                    context.Language,
                    context.OriginalPartner ?? context.Partner,
                    context.ClassicProduct,
                    context.BillableAccountId,
                    context.PartnerSetting);

                return clientAction;
            }
            else
            {
                RedirectionServiceLink redirectLink = GetRDSLink(context);
                ClientAction clientAction = new ClientAction(ClientActionType.Redirect);
                clientAction.Context = redirectLink;
                return clientAction;
            }
        }

        private static RedirectionServiceLink GetRDSLink(PIDLGeneratorContext context)
        {
            if (context.OperationType.Equals(V7.Constants.PidlOperationTypes.Add, StringComparison.OrdinalIgnoreCase))
            {
                    return GetRDSLinkForAddPI(context.PaymentInstrument);
            }

            if (context.OperationType.Equals(V7.Constants.PidlOperationTypes.HandlePaymentChallenge, StringComparison.OrdinalIgnoreCase))
            {
                return GetRDSLinkForIndia3dsHandlePaymentChallenge(context);
            }

            return new RedirectionServiceLink() { BaseUrl = context.RedirectUrl };
        }

        private static RedirectionServiceLink GetRDSLinkForAddPI(PaymentInstrument paymentInstrument)
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

        private static RedirectionServiceLink GetRDSLinkForIndia3dsHandlePaymentChallenge(PIDLGeneratorContext context)
        {
            RedirectionServiceLink redirectLink = new RedirectionServiceLink { BaseUrl = context.RedirectUrl };
            redirectLink.RuParameters.Add("sessionId", context.SessionId);
            redirectLink.RxParameters.Add("sessionId", context.SessionId);

            return redirectLink;
        }
    }
}