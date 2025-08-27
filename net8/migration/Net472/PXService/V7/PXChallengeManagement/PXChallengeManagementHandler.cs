// <copyright file="PXChallengeManagementHandler.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>
namespace Microsoft.Commerce.Payments.PXService.V7.PXChallengeManagement
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.Eventing.Reader;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Web;
    using Microsoft.Commerce.Payments.Common.Tracing;
    using Microsoft.Commerce.Payments.Common.Web;
    using Microsoft.Commerce.Payments.PidlFactory.V7;
    using Microsoft.Commerce.Payments.PidlModel.V7;
    using Microsoft.Commerce.Payments.PXCommon;
    using Microsoft.Commerce.Payments.PXService.Accessors.ChallengeManagementService;
    using Microsoft.Commerce.Payments.PXService.Model.ChallengeManagementService;
    using Microsoft.Commerce.Tracing;
    using Newtonsoft.Json;
    using static Microsoft.Commerce.Payments.PXService.Model.ChallengeManagementService.SessionEnumDefinition;

    public class PXChallengeManagementHandler
    {
        private IChallengeManagementServiceAccessor challengeManagementServiceAccessor;

        public PXChallengeManagementHandler(IChallengeManagementServiceAccessor challengeManagementServiceAccessor)
        {
            this.challengeManagementServiceAccessor = challengeManagementServiceAccessor;
        }

        public async Task<string> CreatePXChallengeSessionId(Dictionary<string, string> queryParams, string accountId, EventTraceActivity traceActivityId, Version pidlSdkVersion = null)
        {
            try
            {
                var sessionDataModel = new SessionDataModel()
                {
                    Language = queryParams.ContainsKey("language") ? queryParams["language"] : string.Empty,
                    Partner = queryParams.ContainsKey("partner") ? queryParams["partner"] : string.Empty,
                    Country = queryParams.ContainsKey("country") ? queryParams["country"] : string.Empty,
                    Operation = queryParams.ContainsKey("operation") ? queryParams["operation"] : string.Empty,
                    Family = queryParams.ContainsKey("family") ? queryParams["family"] : string.Empty,
                    CardType = queryParams.ContainsKey("type") ? queryParams["type"] : string.Empty,
                    AccountId = !string.IsNullOrEmpty(accountId) ? accountId : string.Empty,
                    PidlSdkVersion = pidlSdkVersion != null ? pidlSdkVersion.ToString() : string.Empty
                };
                var sessionData = JsonConvert.SerializeObject(sessionDataModel);
                var sessionResponse = await this.challengeManagementServiceAccessor.CreateChallengeSession(sessionData, traceActivityId);
                return sessionResponse.SessionId;
            }
            catch (Exception ex)
            {
                SllWebLogger.TracePXServiceException($"Error occured while creating PX Session, Error Message {ex.Message}.", traceActivityId);
                return null;
            }
        }

        public async Task AddChallenge(List<PIDLResource> pidlResources, EventTraceActivity traceActivity, string partner, string sessionId, string language, List<string> exposedFlightFeatures = null)
        {
            try
            {
                var challengeResponsePIDL = await this.challengeManagementServiceAccessor.CreateChallenge(sessionId, traceActivity, language, GetFlightedRiskScore(exposedFlightFeatures), GetFlightedChallengeProvider(exposedFlightFeatures).ToString());
                var translatedChallengeLinkedPIDLs = new List<PIDLResource>();
                PIDLResource.PopulatePIDLResource(challengeResponsePIDL, translatedChallengeLinkedPIDLs);
                var challengeLinkedPidl = translatedChallengeLinkedPIDLs.FirstOrDefault();

                if (exposedFlightFeatures != null && exposedFlightFeatures.Contains(Flighting.Features.PXChallengeMultipageChallenge, StringComparer.OrdinalIgnoreCase))
                {
                    SllWebLogger.TraceServerMessage("PX-AddPI-CMSHandlerAddChallenge", traceActivity.CorrelationVectorV4.Value, traceActivity.ActivityId.ToString(), $"Starting Multipage pidl transformation for PXChallengeSessionId {sessionId}", Diagnostics.Tracing.EventLevel.Informational);
                    
                    var challengePageDisplayHint = (PageDisplayHint)challengeLinkedPidl.GetDisplayHintById(V7.Constants.DisplayHintIds.LoadArkoseChallenge);
                    var backButton = CreateBackButtonWithNextAction();
                    var saveButton = GetSaveButtonFromPidl(pidlResources[0]);
                    var backSaveGroup = new GroupDisplayHint()
                    {
                        HintId = V7.Constants.DisplayHintIds.BackSaveGroup,
                        LayoutOrientation = "inline",
                        IsSumbitGroup = true
                    };
                    backSaveGroup.AddDisplayHint(backButton);
                    backSaveGroup.AddDisplayHint(saveButton);
                    challengePageDisplayHint.AddDisplayHint(backSaveGroup);

                    var nextButton = CreateNextButtonWithFirstAction();

                    foreach (var pidlResource in pidlResources)
                    {
                        var saveButtonToBeRemoved = GetSaveButtonFromPidl(pidlResource);
                        var saveButtonHintId = saveButtonToBeRemoved != null ? saveButton.HintId : string.Empty;
                        var submitGroup = pidlResource.GetParentGroupForDisplayHint(saveButtonHintId);
                        pidlResource.InsertDisplayPageAtIndex(0, challengePageDisplayHint);
                        if (submitGroup != null)
                        {
                            submitGroup.AddDisplayHint(nextButton);
                            submitGroup.RemoveDisplayHint(saveButtonToBeRemoved);
                        }
                    }

                    SllWebLogger.TraceServerMessage("PX-AddPI-CMSHandlerAddChallenge", traceActivity.CorrelationVectorV4.Value, traceActivity.ActivityId.ToString(), $"Completed Multipage PIDL transformation for PXChallengeSessionId {sessionId}", Diagnostics.Tracing.EventLevel.Informational);
                }
                else
                {
                    challengeLinkedPidl.MakeSecondaryResource();
                    challengeLinkedPidl.SetErrorHandlingToIgnore();
                    PIDLResourceFactory.AddLinkedPidlToResourceList(pidlResources, challengeLinkedPidl, partner, PidlContainerDisplayHint.SubmissionOrder.BeforeBase);
                }
            }
            catch (Exception ex)
            {
                SllWebLogger.TracePXServiceException($"Error occured while creating PX Challenge. PXChallengeSessionId {sessionId}, Error Message {ex.Message}.", traceActivity);
                throw ex;
            }
        }

        public async Task<Dictionary<string, bool>> GetPXChallengeSession(string pxChallengeSessionId, string accountId, EventTraceActivity traceActivityId)
        {
            try
            {
                SessionBusinessModel pxSession = await this.challengeManagementServiceAccessor.GetChallengeSession(pxChallengeSessionId, traceActivityId);
                if (pxSession != null && pxSession.SessionData != null)
                {
                    var sessionData = JsonConvert.DeserializeObject<SessionDataModel>(pxSession.SessionData);
                    var response = new Dictionary<string, bool>()
                    {
                        { "isPXChallengeSessionActive", string.Equals(pxSession.Status, SessionEnumDefinition.SessionStatus.Active.ToString(), StringComparison.OrdinalIgnoreCase) },
                        { "isPXChallengeSessionAccountValid", string.Equals(sessionData.AccountId, accountId, StringComparison.OrdinalIgnoreCase) }
                    };

                    return response;
                }

                return null;
            }
            catch (Exception ex)
            {
                SllWebLogger.TracePXServiceException($"Error occured while getting PX Session status. PXChallengeSessionId {pxChallengeSessionId}, Error Message {ex.Message}.", traceActivityId);
                return null;
            }
        }

        public async Task<bool> GetPXChallengeStatus(string pxChallengeSessionId, EventTraceActivity traceActivityId)
        {
            var challengeStatusResult = await this.challengeManagementServiceAccessor.GetChallengeStatus(pxChallengeSessionId, traceActivityId);
            return challengeStatusResult.Passed;
        }

        public async Task<bool> UpdatePXSessionCompletedStatus(string pxChallengeSessionId, EventTraceActivity traceActivityId)
        {
            SessionBusinessModel sessionRequest = new SessionBusinessModel()
            {
                SessionType = SessionEnumDefinition.SessionType.PXAddPISession.ToString(),
                SessionId = pxChallengeSessionId,
                Status = SessionStatus.Completed.ToString()
            };
            var updatedSession = await this.challengeManagementServiceAccessor.UpdateChallengeSession(sessionRequest, traceActivityId);
            if (updatedSession != null)
            {
                return updatedSession.Status == SessionStatus.Completed.ToString();
            }
            else
            {
                return false;
            }
        }

        public async Task<bool> UpdatePXSessionAbandonedStatus(string pxChallengeSessionId, EventTraceActivity traceActivityId)
        {
            SessionBusinessModel sessionRequest = new SessionBusinessModel()
            {
                SessionType = SessionEnumDefinition.SessionType.PXAddPISession.ToString(),
                SessionId = pxChallengeSessionId,
                Status = SessionStatus.Abandoned.ToString()
            };
            var updatedSession = await this.challengeManagementServiceAccessor.UpdateChallengeSession(sessionRequest, traceActivityId);
            if (updatedSession != null)
            {
                return updatedSession.Status == SessionStatus.Abandoned.ToString();
            }
            else
            {
                return false;
            }
        }

        public async Task<bool> UpdatePXSessionChallengeRequired(string pxChallengeSessionId, bool isChallengeRequired, EventTraceActivity traceActivityId)
        {
            SessionBusinessModel pxSession = await this.challengeManagementServiceAccessor.GetChallengeSession(pxChallengeSessionId, traceActivityId);
            var sessionData = JsonConvert.DeserializeObject<SessionDataModel>(pxSession.SessionData);
            sessionData.ChallengeRequired = isChallengeRequired;
            pxSession.SessionData = JsonConvert.SerializeObject(sessionData);

            var updatedSession = await this.challengeManagementServiceAccessor.UpdateChallengeSession(pxSession, traceActivityId);
            if (updatedSession != null && updatedSession.SessionData != null)
            {
                var updatedSessionData = JsonConvert.DeserializeObject<SessionDataModel>(updatedSession.SessionData);
                return updatedSessionData.ChallengeRequired;
            }
            else
            {
                return false;
            }
        }

        private static int GetFlightedRiskScore(List<string> exposedFlightFeatures = null)
        {
            if (exposedFlightFeatures != null)
            {
                if (exposedFlightFeatures.Contains(Flighting.Features.PXChallengeComplexityLow))
                {
                    return 30;
                }
                else if (exposedFlightFeatures.Contains(Flighting.Features.PXChallengeComplexityHigh))
                {
                    return 90;
                }
            }

            return 50;
        }

        private static ChallengeEnumDefinition.ChallengeProvider GetFlightedChallengeProvider(List<string> exposedFlightFeatures = null)
        {
            if (exposedFlightFeatures != null)
            {
                if (exposedFlightFeatures.Contains(Flighting.Features.PXChallengeProviderEnableHIP))
                {
                    return ChallengeEnumDefinition.ChallengeProvider.HIP;
                }
            }

            return ChallengeEnumDefinition.ChallengeProvider.Arkose;
        }

        private static ButtonDisplayHint CreateBackButtonWithNextAction()
        {
            ButtonDisplayHint backButton = new ButtonDisplayHint()
            {
                HintId = V7.Constants.ButtonDisplayHintIds.BackButton,
                Action = new DisplayHintAction(DisplayHintActionType.moveNext.ToString()),
                DisplayContent = PidlModelHelper.GetLocalizedString(V7.Constants.ChallengeManagementDisplayLabels.BackButtonLabel),
            };
            backButton.AddDisplayTag("accessibilityName", PidlModelHelper.GetLocalizedString(V7.Constants.ChallengeManagementDisplayLabels.BackButtonLabel));
            return backButton;
        }

        private static ButtonDisplayHint CreateNextButtonWithFirstAction()
        {
            ButtonDisplayHint nextButton = new ButtonDisplayHint()
            {
                HintId = V7.Constants.ButtonDisplayHintIds.NextButton,
                Action = new DisplayHintAction(DisplayHintActionType.moveFirst.ToString()),
                DisplayContent = PidlModelHelper.GetLocalizedString(V7.Constants.ChallengeManagementDisplayLabels.NextButtonLabel),
            };
            nextButton.AddDisplayTag("accessibilityName", PidlModelHelper.GetLocalizedString(V7.Constants.ChallengeManagementDisplayLabels.NextButtonLabel));
            return nextButton;
        }

        private static DisplayHint GetSaveButtonFromPidl(PIDLResource pidlResource)
        {
            DisplayHint saveButton = pidlResource.GetDisplayHintById(V7.Constants.DisplayHintIds.SaveButton);
            if (saveButton != null)
            {
                return saveButton;
            }
            else
            {
                saveButton = pidlResource.GetDisplayHintById(V7.Constants.DisplayHintIds.SaveNextButton);
                if (saveButton != null)
                {
                    return saveButton;
                }
                else
                {
                    saveButton = pidlResource.GetDisplayHintById(V7.Constants.DisplayHintIds.SaveConfirmButton);
                    if (saveButton != null)
                    {
                        return saveButton;
                    }
                    else
                    {
                        return null;
                    }
                }
            }
        }
    }
}