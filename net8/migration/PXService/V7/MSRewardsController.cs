// <copyright file="MSRewardsController.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.V7
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Commerce.Payments.Common.Tracing;
    using Microsoft.Commerce.Payments.Common.Web;
    using Microsoft.Commerce.Payments.PartnerSettingsModel;
    using Microsoft.Commerce.Payments.PidlFactory.V7;
    using Microsoft.Commerce.Payments.PidlFactory.V7.PIDLFeatureProcess;
    using Microsoft.Commerce.Payments.PidlModel.V7;
    using Microsoft.Commerce.Payments.PimsModel.V4;
    using Microsoft.Commerce.Payments.PXCommon;
    using Microsoft.Commerce.Payments.PXService.Accessors.MSRewardsService.DataModel;
    using Microsoft.Commerce.Payments.PXService.Model.RewardsService;

    public class MSRewardsController : ProxyController
    {
        /// <summary>
        /// Post MSRewards redeem request
        /// </summary>
        /// <group>MSRewards</group>
        /// <verb>POST</verb>
        /// <url>https://paymentexperience.cp.microsoft.com/v7.0/{accountId}/msRewards</url>
        /// <param name="redeemData" required="true" cref="object" in="query">Redeem request data</param>
        /// <param name="accountId" required="true" cref="string" in="path">Account ID</param>
        /// <param name="country" required="true" cref="string" in="query">country code</param>
        /// <param name="language" required="true" cref="string" in="query">language code</param>
        /// <param name="partner" required="false" cref="string" in="query">Partner name</param>
        /// <response code="200">Redeem Response object</response>
        /// <returns>Redeem Response object</returns>
        [HttpPost]
        public async Task<PIDLResource> PostRedeemRequest(
            [FromBody] MSRewardsRedeemRequest redeemData,
            [FromQuery] string accountId,
            [FromQuery] string country = null,
            [FromQuery] string language = null,
            [FromQuery] string partner = Constants.ServiceDefaults.DefaultPartnerName)
        {
            EventTraceActivity traceActivityId = this.Request.GetRequestCorrelationId();
            this.Request.AddTracingProperties(accountId, null, null, null);
            this.Request.AddPartnerProperty(partner?.ToLower());

            // Extract user context from the request
            string userId = await this.TryGetClientContext(GlobalConstants.ClientContextKeys.MsaProfile.Puid);
            string ipAddress = await this.TryGetClientContext(GlobalConstants.ClientContextKeys.DeviceInfo.IPAddress);
            string userAgent = await this.TryGetClientContext(GlobalConstants.ClientContextKeys.DeviceInfo.UserAgent);
            string xboxDeviceId = await this.TryGetClientContext(GlobalConstants.ClientContextKeys.DeviceInfo.XboxLiveDeviceId);
            string deviceId = await this.TryGetClientContext(GlobalConstants.ClientContextKeys.DeviceInfo.DeviceId);

            PaymentExperienceSetting setting = this.GetPaymentExperienceSetting(Constants.Operations.Redeem);

            // Prepare redemption request
            RedemptionRequest redemptionRequest = new RedemptionRequest();
            redemptionRequest.CatalogItem = redeemData.CatalogItem; // redeemData.CatalogItem should exist for all the requests

            // If CatalogItemAmount is present, set the variable redemption request
            if (redeemData.CatalogItemAmount.HasValue)
            {
                int variableAmount = 0;
                try
                {
                    variableAmount = decimal.ToInt32(redeemData.CatalogItemAmount.Value);
                }
                catch (Exception ex)
                {
                    SllWebLogger.TracePXServiceException($"Failed to CatalogItemAmount to integer. Error: {ex.Message}", traceActivityId);
                }

                if (variableAmount > 0)
                {
                    redemptionRequest.VariableRedemptionRequest = new VariableRedemptionItemRequestDetails()
                    {
                        VariableAmount = variableAmount
                    };
                }
            }

            RiskVerificationType challengePreference;
            Enum.TryParse(redeemData.ChallengePreference, true, out challengePreference); // redeemData.ChallengePreference should exist when sms or call option is chosen by the user for challenge scenarios
            redemptionRequest.RiskContext = new RiskOrderContext()
            {
                ChallengePreference = challengePreference,
                SolveCode = redeemData.SolveCode, // redeemData.SolveCode should exist when challenge code is submitted by the user
                ChallengeToken = redeemData.ChallengeToken, // redeemData.ChallengeToken should exist when challenge code is submitted by the user
                UiLanguage = language,
                DeviceId = deviceId ?? xboxDeviceId ?? redeemData.DeviceId,
                DeviceType = redeemData.DeviceType,
                IpAddress = ipAddress,
                UserAgent = userAgent,
                GreenId = redeemData.GreenId
            };

            List<KeyValuePair<string, string>> queryParams = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>(Constants.QueryParameterName.Country, country),
            };

            // If PXEnableMSRewardsChallenge flight is enabled, trigger friction
            bool hasAnyStoredPI = false;
            if (!this.ExposedFlightFeatures.Contains(Flighting.Features.PXEnableMSRewardsChallenge, StringComparer.OrdinalIgnoreCase))
            {
                // Check if the user has any stored payment instrument
                hasAnyStoredPI = await PIHelper.HasAnyStoredPI(this.Settings, accountId, partner, country, language, traceActivityId, exposedFlightFeatures: this.ExposedFlightFeatures);
            }

            if (this.ExposedFlightFeatures?.Contains(Flighting.Features.PXOverrideHasAnyPIToTrue, StringComparer.OrdinalIgnoreCase) ?? false)
            {
                // set this to true if the override flight is enabled
                hasAnyStoredPI = true;
            }

            PIDLResource retVal = new PIDLResource();
            ClientAction clientAction = new ClientAction(ClientActionType.Pidl);
            List<PIDLResource> retList = null;
            RedemptionResult redemptionResult = null;
            try
            {
                // Call the MS Rewards service to redeem the rewards
                redemptionResult = await this.Settings.MSRewardsServiceAccessor.RedeemRewards(userId, country, partner, hasAnyStoredPI, redemptionRequest, traceActivityId);
            }
            catch (Exception ex)
            {
                if (this.ExposedFlightFeatures?.Contains(Flighting.Features.PXShowRewardsErrorPage, StringComparer.OrdinalIgnoreCase) ?? false)
                {
                    retList = PIDLResourceFactory.Instance.GetRewardsDescriptions(Constants.DescriptionTypes.MSRewards, Constants.RewardsOperations.Error, country, language, partner);
                    string errorMessage = "Failed to redeem rewards. Please try again later.";
                    retList = ProcessErrorPIDL(retList, RewardsErrorCode.E_REWARDS_GENERIC_ERROR, errorMessage);
                }
                else
                {
                    // If flight is not enabled, throw the original exception as-is
                    throw ex;
                }
            }

            if (redemptionResult != null)
            {
                // Show the failure code as it is returned from the service as default
                // this will be overwritten based on the redemption result code
                clientAction = new ClientAction(ClientActionType.Failure)
                {
                    Context = new { errorCode = redemptionResult.Code, errorCodeName = redemptionResult.Code.ToString(), errorMessage = redemptionResult.ResultMessage }
                };

                // Extract the order details from the redemption result
                string orderId = redemptionResult.Order?.OrderId;
                string sku = redemptionResult.Order?.OrderSKU;
                long? redeemPoints = redemptionResult.Order?.ItemSnapshot?.Price;
                string redeemAmount = null;
                redemptionResult.Order?.ItemSnapshot?.Configuration?.TryGetValue("amount", out redeemAmount);
                string redeemCurrency = null;
                redemptionResult.Order?.ItemSnapshot?.Configuration?.TryGetValue("currencyCode", out redeemCurrency);

                string isVariableAmountVal = null;
                if ((redemptionResult.Order?.Attributes?.TryGetValue("isVariableAmount", out isVariableAmountVal) ?? false)
                    && string.Equals(isVariableAmountVal, "true", System.StringComparison.OrdinalIgnoreCase))
                {
                    redemptionResult.Order?.Attributes?.TryGetValue("amount", out redeemAmount);
                    redeemPoints = redemptionResult.Order?.Price;
                }

                bool showErrorPageOnChallenge = false;
                switch (redemptionResult.Code)
                {
                    case RewardsErrorCode.Success:
                        // Show the client action with type as "ReturnContext" and context as "status" : "Completed"
                        // Get updated CSV PI details and return the context
                        PaymentInstrument csvPI = await PIHelper.GetCSVPI(this.Settings, accountId, partner, country, language, traceActivityId, exposedFlightFeatures: this.ExposedFlightFeatures);
                        clientAction = new ClientAction(ClientActionType.ReturnContext)
                        {
                            Context = new { status = "Completed", orderId, sku, redeemPoints, redeemAmount, redeemCurrency, csvPIID = csvPI?.PaymentInstrumentId, csvBalance = csvPI?.PaymentInstrumentDetails?.Balance, csvCurrency = csvPI?.PaymentInstrumentDetails?.Currency }
                        };
                        break;
                    case RewardsErrorCode.E_CHALLENGE_FIRST:
                        if (this.ExposedFlightFeatures?.Contains(Flighting.Features.PXShowRewardsErrorPage, StringComparer.OrdinalIgnoreCase) ?? false)
                        {
                            showErrorPageOnChallenge = true;
                            break;
                        }

                        string phoneNumber = redemptionResult.MsaPhoneNumber;
                        if (phoneNumber == null)
                        {
                            // If phoneNumber is empty or null, show the QR code PIDL pointing to aka.ms/editPhone
                            retList = PIDLResourceFactory.Instance.GetEditPhoneQRCodeDescriptions(language, partner);
                            retList = ProcessEditPhonePIDL(retList, country, language, partner, redeemData.CatalogItem, redeemData.CatalogItemAmount);
                        }
                        else
                        {
                            // Show the client action with type as "PIDL" and context as challenge screen PIDL asking user to pick SMS or Call
                            retList = PIDLResourceFactory.Instance.GetRewardsDescriptions(Constants.DescriptionTypes.MSRewards, Constants.RewardsOperations.SelectChallengeType, country, language, partner);
                            retList = ProcessChallengePIDL(retList, country, language, partner, redeemData.CatalogItem, redeemData.CatalogItemAmount, phoneNumber);
                        }

                        break;
                    case RewardsErrorCode.E_SOLVE_FIRST:
                        if (this.ExposedFlightFeatures?.Contains(Flighting.Features.PXShowRewardsErrorPage, StringComparer.OrdinalIgnoreCase) ?? false)
                        {
                            showErrorPageOnChallenge = true;
                            break;
                        }

                        // Show the client action with type as "PIDL" and context as challenge screen PIDL asking user to enter the solve code
                        string challengeToken = redemptionResult.ResultMessage;
                        retList = PIDLResourceFactory.Instance.GetRewardsDescriptions(Constants.DescriptionTypes.MSRewards, Constants.RewardsOperations.SubmitChallengeCode, country, language, partner);
                        retList = ProcessSolveCodePIDL(retList, country, language, partner, redeemData.CatalogItem, redeemData.CatalogItemAmount, challengeToken, redeemData.ChallengePreference);
                        break;
                    case RewardsErrorCode.E_RISK_REVIEW:
                        if (this.ExposedFlightFeatures?.Contains(Flighting.Features.PXShowRewardsErrorPage, StringComparer.OrdinalIgnoreCase) ?? false)
                        {
                            showErrorPageOnChallenge = true;
                            break;
                        }

                        // Show the client action with type as "ReturnContext" and context as "status" : "PendingRiskReview"
                        clientAction = new ClientAction(ClientActionType.ReturnContext)
                        {
                            Context = new { status = "PendingRiskReview", orderId, sku, redeemPoints, redeemAmount, redeemCurrency }
                        };
                        break;
                    default:
                        if (this.ExposedFlightFeatures?.Contains(Flighting.Features.PXShowRewardsErrorPage, StringComparer.OrdinalIgnoreCase) ?? false)
                        {
                            showErrorPageOnChallenge = true;
                        }

                        break;
                }

                if (showErrorPageOnChallenge)
                {
                    retList = PIDLResourceFactory.Instance.GetRewardsDescriptions(Constants.DescriptionTypes.MSRewards, Constants.RewardsOperations.Error, country, language, partner);
                    retList = ProcessErrorPIDL(retList, redemptionResult.Code, redemptionResult.ResultMessage);
                }
            }

            if (retList != null && retList.Count > 0)
            {
                FeatureContext featureContext = new FeatureContext(
                    country,
                    GetSettingTemplate(partner, setting, Constants.DescriptionTypes.MSRewards, null, null),
                    Constants.DescriptionTypes.MSRewards,
                    Constants.Operations.Redeem,
                    scenario: null,
                    language,
                    null,
                    this.ExposedFlightFeatures,
                    setting?.Features,
                    paymentMethodfamily: null,
                    null,
                    smdMarkets: null,
                    originalPartner: partner,
                    isGuestAccount: GuestAccountHelper.IsGuestAccount(this.Request));

                PostProcessor.Process(retList, PIDLResourceFactory.FeatureFactory, featureContext);
                clientAction = new ClientAction(ClientActionType.Pidl)
                {
                    Context = retList
                };
            }

            retVal.ClientAction = clientAction;
            return retVal;
        }

        private static List<PIDLResource> ProcessSolveCodePIDL(List<PIDLResource> pidlResources, string country, string language, string partner, string catalogItem, decimal? catalogItemAmount, string challengeToken, string challengePreference)
        {
            Dictionary<string, object> defaultValueToBeUpdated = new Dictionary<string, object>()
            {
                { Constants.PropertyDescriptionIds.CatalogItem, catalogItem },
                { Constants.PropertyDescriptionIds.ChallengeToken, challengeToken },
                { Constants.PropertyDescriptionIds.ChallengePreference, challengePreference },
            };

            if (catalogItemAmount.HasValue)
            {
                defaultValueToBeUpdated[Constants.PropertyDescriptionIds.CatalogItemAmount] = catalogItemAmount.Value;
            }

            foreach (var pidlResource in pidlResources)
            {
                foreach (var item in defaultValueToBeUpdated)
                {
                    var dataDescription = pidlResource.DataDescription[item.Key] as PropertyDescription;
                    if (dataDescription != null)
                    {
                        dataDescription.DefaultValue = item.Value;
                    }
                }

                // Add a PidlAction to the Resend Code button
                object payload = new { catalogItem, catalogItemAmount, challengePreference };
                var resendCodeButton = pidlResource.GetDisplayHintById(Constants.DisplayHintIds.MSRewardsNewCodeButton) as ButtonDisplayHint;
                if (resendCodeButton != null)
                {
                    resendCodeButton.Action = MakeMSRewardsPostAction(partner, language, country, payload, DisplayHintActionType.restAction.ToString());
                }

                // Add a PidlAction to the submit button
                var submitButton = pidlResource.GetDisplayHintById(Constants.DisplayHintIds.ConfirmButton) as ButtonDisplayHint;
                if (submitButton != null)
                {
                    submitButton.Action = MakeMSRewardsPostAction(partner, language, country, null, DisplayHintActionType.submit.ToString());
                }
            }

            return pidlResources;
        }

        private static List<PIDLResource> ProcessChallengePIDL(List<PIDLResource> pidlResources, string country, string language, string partner, string catalogItem, decimal? catalogItemAmount, string phoneNumber)
        {
            Dictionary<string, object> defaultValueToBeUpdated = new Dictionary<string, object>()
            {
                { Constants.PropertyDescriptionIds.CatalogItem, catalogItem },
                { Constants.PropertyDescriptionIds.PhoneNumber, phoneNumber }
            };

            if (catalogItemAmount.HasValue)
            {
                defaultValueToBeUpdated[Constants.PropertyDescriptionIds.CatalogItemAmount] = catalogItemAmount.Value;
            }

            foreach (var pidlResource in pidlResources)
            {
                // Add default values to the data descrioptions
                foreach (var item in defaultValueToBeUpdated)
                {
                    var dataDescription = pidlResource.DataDescription[item.Key] as PropertyDescription;
                    if (dataDescription != null)
                    {
                        dataDescription.DefaultValue = item.Value;
                    }
                }

                // Add a PidlAction to the submit button
                var sendCodeButton = pidlResource.GetDisplayHintById(Constants.DisplayHintIds.RedeemMSRewardsPhoneChallengeSendCodeButton) as ButtonDisplayHint;
                if (sendCodeButton != null)
                {
                    sendCodeButton.Action = MakeMSRewardsPostAction(partner, language, country, null, DisplayHintActionType.submit.ToString());
                }

                // Add a PidlAction to the submit button on QrCode page
                var payload = new { catalogItem, catalogItemAmount };
                var continueSubmitButton = pidlResource.GetDisplayHintById(Constants.DisplayHintIds.ContinueSubmitButton) as ButtonDisplayHint;
                if (continueSubmitButton != null)
                {
                    continueSubmitButton.Action = MakeMSRewardsPostAction(partner, language, country, payload, DisplayHintActionType.submit.ToString());
                }

                // On clicking cancel from qr code page, move to the select challenge type page
                var cancelButton = pidlResource.GetDisplayHintById(Constants.DisplayHintIds.CancelButton) as ButtonDisplayHint;
                if (cancelButton != null)
                {
                    cancelButton.Action.ActionType = DisplayHintActionType.moveFirst.ToString();
                }
            }

            return pidlResources;
        }

        private static List<PIDLResource> ProcessEditPhonePIDL(List<PIDLResource> pidlResources, string country, string language, string partner, string catalogItem, decimal? catalogItemAmount)
        {
            Dictionary<string, object> defaultValueToBeUpdated = new Dictionary<string, object>()
            {
                { Constants.PropertyDescriptionIds.CatalogItem, catalogItem },
            };

            if (catalogItemAmount.HasValue)
            {
                defaultValueToBeUpdated[Constants.PropertyDescriptionIds.CatalogItemAmount] = catalogItemAmount.Value;
            }

            foreach (var pidlResource in pidlResources)
            {
                // Add default values to the data descrioptions
                foreach (var item in defaultValueToBeUpdated)
                {
                    var dataDescription = pidlResource.DataDescription[item.Key] as PropertyDescription;
                    if (dataDescription != null)
                    {
                        dataDescription.DefaultValue = item.Value;
                    }
                }

                // Add a PidlAction to the submit button
                var continueSubmitButton = pidlResource.GetDisplayHintById(Constants.DisplayHintIds.ContinueSubmitButton) as ButtonDisplayHint;
                if (continueSubmitButton != null)
                {
                    continueSubmitButton.Action = MakeMSRewardsPostAction(partner, language, country, null, DisplayHintActionType.submit.ToString());
                }
            }

            return pidlResources;
        }

        private static DisplayHintAction MakeMSRewardsPostAction(string partner, string language, string country, object payload, string actionType)
        {
            var resendCodeActionContext = new PXCommon.RestLink();
            resendCodeActionContext.Href = $"{Constants.SubmitUrls.RedeemMSRewards}?partner={partner}&language={language}&country={country}";
            resendCodeActionContext.Method = "POST";
            if (payload != null)
            {
                resendCodeActionContext.Payload = payload;
            }

            return new DisplayHintAction(actionType, false, resendCodeActionContext, null);
        }

        private static List<PIDLResource> ProcessErrorPIDL(List<PIDLResource> pidlResources, RewardsErrorCode code, string errorMessage)
        {
            PidlModel.V7.ActionContext actionContext = new PidlModel.V7.ActionContext();
            actionContext.Instance = new
            {
                errorCode = code.ToString(),
                errorMessage,
                status = "Failed"
            };

            DisplayHintAction action = new DisplayHintAction("success", true, actionContext, null);
            foreach (var pidlResource in pidlResources)
            {
                TextDisplayHint errorBadgeText = pidlResource.GetDisplayHintById(Constants.DisplayHintIds.ErrorBadgeText) as TextDisplayHint;
                if (errorBadgeText != null)
                {
                    errorBadgeText.DisplayContent = Constants.FontIcons.ErrorBadge;
                }

                // Add a PidlAction to the submit button
                var gotItButton = pidlResource.GetDisplayHintById(Constants.DisplayHintIds.GotItButton) as ButtonDisplayHint;
                if (gotItButton != null)
                {
                    gotItButton.Action = action;
                }
            }

            return pidlResources;
        }
    }
}