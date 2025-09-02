// <copyright file="PaymentRequestConfirmDescription.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.Tracing;
    using System.Linq;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Microsoft.Commerce.Payments.Common;
    using Microsoft.Commerce.Payments.Common.Tracing;
    using Microsoft.Commerce.Payments.Common.Web;
    using Microsoft.Commerce.Payments.PartnerSettingsModel;
    using Microsoft.Commerce.Payments.PidlFactory.V7;
    using Microsoft.Commerce.Payments.PidlModel.V7;
    using Microsoft.Commerce.Payments.PXService.Model.PaymentOrchestratorService;
    using Microsoft.Commerce.Payments.PXService.Model.ThreeDSExternalService;
    using Microsoft.Commerce.Payments.PXService.Settings;
    using Microsoft.Commerce.Payments.PXService.V7.Contexts;
    using Microsoft.Commerce.Payments.PXService.V7.PaymentChallenge.Model;
    using static Microsoft.Commerce.Payments.PXService.V7.Constants;

    public class PaymentRequestConfirmDescription : ComponentDescription
    {
        private const string PIDLDescriptionType = DescriptionTypes.ChallengeDescription;
        private const string PaymentInstrumentIdKey = "piId";        
        private EventTraceActivity activityId;
        private RequestContext requestContext;
        private Generate3DS2ChallengePIDLResource generate3DS2ChallengePIDLResource;

        public PaymentRequestConfirmDescription(RequestContext requestContext, Generate3DS2ChallengePIDLResource generate3DS2ChallengePIDLResource)
        {
            if (requestContext == null)
            {
                throw new ArgumentNullException(nameof(requestContext));
            }            
            
            if (generate3DS2ChallengePIDLResource == null)
            {
                throw new ArgumentNullException(nameof(generate3DS2ChallengePIDLResource));
            }

            this.requestContext = requestContext;
            this.generate3DS2ChallengePIDLResource = generate3DS2ChallengePIDLResource;
        }

        public override string DescriptionType
        {
            get
            {
                return PIDLDescriptionType;
            }
        }

        public virtual async Task LoadComponentDescription(
            string requestId,
            PXServiceSettings pxSettings,
            EventTraceActivity traceActivityId,
            PaymentExperienceSetting setting,
            List<string> exposedFlightFeatures,
            string operation = null,
            string partner = null,
            string family = null,
            string type = null,
            string scenario = null,
            string country = null,
            string language = null,
            string currency = null,
            HttpRequestMessage request = null,
            string piid = null,
            string challengeWindowSize = null)
        {
            this.Operation = operation;
            this.Partner = partner;
            this.Country = country;
            this.Language = language;
            this.Currency = currency;
            this.Scenario = scenario;
            this.ExposedFlightFeatures = exposedFlightFeatures;
            this.PSSSetting = setting;
            this.PXSettings = pxSettings;
            this.RequestId = requestId;
            this.activityId = traceActivityId;
            this.ChallengeWindowSize = challengeWindowSize;

            if (this.UsePaymentRequestApiEnabled()
                && PidlFactory.V7.PartnerSettingsHelper.IsFeatureEnabledUsingPartnerSettings(PidlFactory.V7.PartnerSettingsHelper.Features.PaymentClientHandlePaymentCollection, country, setting))
            {
                // If the PaymentClientHandlePaymentCollection feature is enabled, we will not need to call PaymentRequestConfirm because the confirm has been called before confirm PIDL is created.
                return;
            }
            else if ((this.UsePaymentRequestApiEnabled()
                && !PidlFactory.V7.PartnerSettingsHelper.IsFeatureEnabledUsingPartnerSettings(PidlFactory.V7.PartnerSettingsHelper.Features.PaymentClientHandlePaymentCollection, country, setting))
                || RequestContext.GetRequestType(requestId) == RequestContextType.Payment)
            {
                var paymentRequestClientActions = await this.PXSettings.PaymentOrchestratorServiceAccessor.PaymentRequestConfirm(requestId, traceActivityId);

                this.Country = paymentRequestClientActions.Country;
                this.Language = paymentRequestClientActions.Language;
                this.PaymentRequestClientActions = paymentRequestClientActions;
            }
            else
            {
                throw TraceCore.TraceException(traceActivityId, new ValidationException(ErrorCode.InvalidRequestData, "Invalid request type for payment request confirm description"));
            }
        }

        public override async Task<List<PIDLResource>> GetDescription()
        {
            if (this.PaymentRequestClientActions == null)
            {
                throw new FailedOperationException($"Failed to get payment request client actions for the given request id {this.RequestId}");
            }

            var clientAction = this.PaymentRequestClientActions.ClientActions?.FirstOrDefault();

            if (clientAction == null)
            {
                return GetClientActionResource(this.PaymentRequestClientActions);
            }

            if (clientAction.Type == ClientActionType.HandleChallenge && clientAction.ChallengeType == PaymentInstrumentChallengeType.Cvv)
            {
                return this.GetChallengePidls(clientAction);
            }
            else if (clientAction.Type == ClientActionType.HandleChallenge && clientAction.ChallengeType == PaymentInstrumentChallengeType.ThreeDs2)
            {
                // Get the 3DS2 challenge for the payment request.
                PIDLResource clientActionFor3DS2 = await this.generate3DS2ChallengePIDLResource(
                    GetPaymentSessionData(this.Partner ?? TemplateName.DefaultTemplate, this.PaymentRequestClientActions),
                    this.requestContext,
                    this.activityId,
                    this.PSSSetting);

                if (clientActionFor3DS2 != null)
                {
                    return new List<PIDLResource>() { clientActionFor3DS2 };
                }
                else
                {
                    SllWebLogger.TraceServerMessage("GetDescription", this.activityId.CorrelationVectorV4.Value, this.activityId.ActivityId.ToString(), "ThreeDs2 challenge is required but clientActionFor3DS2 is null", EventLevel.Warning);
                    return GetClientActionResource(this.PaymentRequestClientActions);
                }
            }
            else
            {
                // ToDo: We need to handle other challenge types like 3DS etc.
                throw new NotImplementedException($"Invalid client action type {clientAction.Type} or challenge type {clientAction.ChallengeType}");
            }
        }

        private static List<PIDLResource> GetClientActionResource(PaymentRequestClientActions paymentRequestClientActions)
        {
            PXCommon.ClientAction pidlClientAction = new PXCommon.ClientAction(PXCommon.ClientActionType.ReturnContext)
            {
                Context = new { response = new RequestStatusResponse { RequestId = paymentRequestClientActions.PaymentRequestId, Status = paymentRequestClientActions.Status } }
            };

            PIDLResource returnContextPidl = new PIDLResource()
            {
                ClientAction = pidlClientAction
            };

            return new List<PIDLResource>() { returnContextPidl };
        }
        private static PaymentSessionData GetPaymentSessionData(string partner, PaymentRequestClientActions paymentRequestClientActions, string challengeWindowSize = null, PaymentExperienceSetting setting = null)
        {
            var clientAction = paymentRequestClientActions.ClientActions?.FirstOrDefault();
            PaymentSessionData paymentSessionData = new PaymentSessionData()
            {
                PaymentInstrumentId = clientAction != null ? clientAction.PaymentInstrument?.PaymentInstrumentId : null,
                Partner = partner,
                Amount = paymentRequestClientActions.Amount,
                Currency = paymentRequestClientActions.Currency,
                Country = paymentRequestClientActions.Country,
                Language = paymentRequestClientActions.Language,
                ChallengeScenario = ChallengeScenario.PaymentTransaction,
                ChallengeWindowSize = ComponentDescription.ParseChallengeWindowSize(challengeWindowSize, setting),
                HasPreOrder = paymentRequestClientActions.PreOrder ?? false,
            };

            return paymentSessionData;
        }

        private List<PIDLResource> GetChallengePidls(PaymentRequestClientAction clientAction)
        {
            var paymentInstrument = clientAction.PaymentInstrument;
            
            if (paymentInstrument == null)
            {
                throw new FailedOperationException($"Failed to get payment instrument for the given request id {this.RequestId}");
            }
            
            List<PIDLResource> retVal = new List<PIDLResource>();
            retVal = PIDLResourceFactory.Instance.GetPaymentClientDescriptions(PIDLDescriptionType, this.Country, ChallengeDescriptionTypes.Cvv, this.Operation, this.Language, this.Partner ?? TemplateName.DefaultTemplate, exposedFlightFeatures: this.ExposedFlightFeatures?.ToList(), scenario: this.Scenario, setting: this.PSSSetting);

            if (retVal == null || retVal.Count == 0)
            {
                throw new FailedOperationException($"Failed to get PIDL resource for the given description type {PIDLDescriptionType}");
            }

            // Add payment instrument id to the pidl to get it back in the payload to attach the challenge data
            retVal[0].DataDescription[PaymentInstrumentIdKey] = new PropertyDescription()
            {
                PropertyType = "userData",
                DataType = "hidden",
                PropertyDescriptionType = "hidden",
                IsUpdatable = false,
                DefaultValue = paymentInstrument.PaymentInstrumentId
            };

            retVal.ForEach(pidl =>
            {
                // ToDo: This should be moved to PSS settings once partner is passed by the payment client
                // Remove Microsoft Account related heading
                pidl.RemoveDisplayHintById(DisplayHintIds.ChallengeCvvHeading);

                //// this is needed for payment client to recognize the pidl as a challenge pidl
                if (pidl?.DisplayPages?.Count > 0)
                {
                    pidl.DisplayPages[0].HintId = "challenge_" + pidl.DisplayPages[0].HintId;
                }

                var cancelBtn = pidl.GetDisplayHintById(ButtonDisplayHintIds.CancelButton);
                cancelBtn.IsHidden = false;

                var nextButton = pidl.GetDisplayHintById(ButtonDisplayHintIds.NextButton);
                nextButton.IsHidden = false;
                nextButton.Action.ActionType = DisplayHintActionType.submit.ToString();

                UpdateSubmitURL(pidl, V7.Constants.ButtonDisplayHintIds.NextButton, GlobalConstants.HTTPVerbs.POST, string.Format(SubmitUrls.PaymentRequestsAttachChallengeData, this.RequestId));
            });

            return retVal;
        }
    }
}