// <copyright file="TokensExController.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.V7
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using Common.Web;
    using Microsoft.Commerce.Payments.Common.Tracing;
    using Microsoft.Commerce.Payments.PartnerSettingsModel;
    using Microsoft.Commerce.Payments.PidlFactory.V7;
    using Microsoft.Commerce.Payments.PidlModel.V7;
    using Microsoft.Commerce.Payments.PXCommon;
    using Microsoft.Commerce.Payments.PXService.Model.NetworkTokenizationService;
    using Newtonsoft.Json;    
    using static Microsoft.Commerce.Payments.PXService.V7.Constants;
    using PaymentInstrument = PimsModel.V4.PaymentInstrument;
    using RestLink = PXCommon.RestLink;

    public class TokensExController : ProxyController
    {
        /// <summary>
        /// Tokens controller
        /// </summary>
        /// <group>Tokens</group>
        /// <verb>Post</verb>
        /// <url>https://paymentexperience.cp.microsoft.com/v7.0/{accountId}/TokensEx</url>
        /// <param name="payload" required="true" cref="object" in="body">Payment instrument</param>
        /// <param name="accountId" required="true" cref="string" in="path">Account ID</param>
        /// <param name="partner" required="true" cref="string" in="query">Partner name</param>
        /// <param name="piid" required="false" cref="string" in="path">Payment instrument id</param>
        /// <param name="country" required="false" cref="string" in="path">two letter country id</param>
        /// <param name="language" required="false" cref="string" in="query">Language code</param>
        /// <response code="200">A list of PIDLResource</response>
        /// <returns>A list of PIDLResource object</returns>
        [HttpPost]
        public async Task<HttpResponseMessage> Tokens([FromBody] PIDLData payload, string accountId, string partner, string piid, string country, string language)
        {
            EventTraceActivity traceActivityId = this.Request.GetRequestCorrelationId();
            PaymentExperienceSetting setting = this.GetPaymentExperienceSetting(V7.Constants.Operations.Get);

            // get pi details from PIMS
            PaymentInstrument paymentInstrument = await this.Settings.PIMSAccessor.GetPaymentInstrument(accountId, piid, traceActivityId, exposedFlightFeatures: this.ExposedFlightFeatures);

            paymentInstrument = await this.Settings.PIMSAccessor.GetExtendedPaymentInstrument(piid, traceActivityId, exposedFlightFeatures: this.ExposedFlightFeatures);

            // gets tokens from network tokenization service
            string puid = await this.TryGetClientContext(GlobalConstants.ClientContextKeys.MsaProfile.Puid);
            string deviceId = await this.TryGetClientContext(GlobalConstants.ClientContextKeys.DeviceInfo.DeviceId);
            string emailAddress = payload.TryGetPropertyValueFromPIDLData(AgenticPaymentRequestData.AccountHolderEmail);
            if (string.IsNullOrWhiteSpace(emailAddress))
            {
                emailAddress = await this.TryGetClientContext(GlobalConstants.ClientContextKeys.MsaProfile.EmailAddress);
            }

            this.ValiateRequestData(emailAddress, AgenticPaymentRequestData.AccountHolderEmail);
            ListTokenMetadataResponse tokens = await this.Settings.NetworkTokenizationServiceAccessor.ListTokensWithExternalCardReference(puid, deviceId, traceActivityId, ExposedFlightFeatures, piid, emailAddress);

            // Extract parameters from payload
            var authenticationAmountStr = payload.TryGetPropertyValueFromPIDLData(AgenticPaymentRequestData.TotalAuthenticationAmount);
            int authenticationAmount = 0;
            int.TryParse(authenticationAmountStr, out authenticationAmount);
            this.ValiateRequestData(authenticationAmount, AgenticPaymentRequestData.TotalAuthenticationAmount);

            var currencyCode = payload.TryGetPropertyValueFromPIDLData(AgenticPaymentRequestData.CurrencyCode);
            this.ValiateRequestData(currencyCode, AgenticPaymentRequestData.CurrencyCode);

            var sessionContext = payload.TryGetPropertyValueFromPIDLData(AgenticPaymentRequestData.SessionContextJsonString);
            this.ValiateRequestData(sessionContext, AgenticPaymentRequestData.SessionContextJsonString);

            var browserData = payload.TryGetPropertyValueFromPIDLData(AgenticPaymentRequestData.BrowserDataJsonString);
            this.ValiateRequestData(browserData, AgenticPaymentRequestData.BrowserDataJsonString);

            var mandate = payload.TryGetPropertyValueFromPIDLData(AgenticPaymentRequestData.MandateJsonString);
            this.ValiateRequestData(mandate, AgenticPaymentRequestData.MandateJsonString);

            var applicationUrl = payload.TryGetPropertyValueFromPIDLData(AgenticPaymentRequestData.ApplicationUrl);
            this.ValiateRequestData(applicationUrl, AgenticPaymentRequestData.ApplicationUrl);

            var merchantName = payload.TryGetPropertyValueFromPIDLData(AgenticPaymentRequestData.MerchantName);
            this.ValiateRequestData(merchantName, AgenticPaymentRequestData.MerchantName);

            // Continue device binding and passkey setup if token for ThirdPartyMerchant usage can be found
            foreach (GetTokenMetadataResponse token in tokens?.Tokens)
            {
                if (token.NetworkTokenUsage == NetworkTokenUsage.EcomMerchant)
                {
                    HttpResponseMessage response = await this.HandleDeviceBindingAndPasskeySetup(token, paymentInstrument, traceActivityId, this.ExposedFlightFeatures, emailAddress, puid, deviceId, language, partner, setting, authenticationAmount, currencyCode, sessionContext, browserData, applicationUrl, merchantName, country, payload);
                    return response;
                }
            }

            // if no token for ThirdPartyMerchant usage can be found, then call tokenizable. If yes, then request token followed by handleDeviceBindingAndPasskeySetup
            GetTokenizationEligibilityResponse tokenizableResponse = await this.Settings.NetworkTokenizationServiceAccessor.Tokenizable(puid, deviceId, traceActivityId, ExposedFlightFeatures, emailAddress, paymentInstrument?.PaymentInstrumentDetails?.BankIdentificationNumber, paymentInstrument?.PaymentMethod?.PaymentMethodType, "EcomMerchant");

            if (tokenizableResponse.Tokenizable)
            {
                GetTokenMetadataResponse token = await this.Settings.NetworkTokenizationServiceAccessor.RequestToken(puid, deviceId, traceActivityId, ExposedFlightFeatures, emailAddress, country, language, paymentInstrument);
                HttpResponseMessage response = await this.HandleDeviceBindingAndPasskeySetup(token, paymentInstrument, traceActivityId, this.ExposedFlightFeatures, emailAddress, puid, deviceId, language, partner, setting, authenticationAmount, currencyCode, sessionContext, browserData, applicationUrl, merchantName, country, payload);
                return response;
            }
            else
            {
                var errorMessage = new ErrorMessage
                {
                    ErrorCode = "NotTokenizable",
                    Message = "TokensEx: PI is not tokenizable: " + piid,
                    Retryable = false
                };

                return new HttpResponseMessage(HttpStatusCode.BadRequest)
                {
                    Content = JsonContent.Create(errorMessage)
                };
            }
        }

        /// <summary>
        /// Tokens controller
        /// </summary>
        /// <group>Tokens</group>
        /// <verb>Post</verb>
        /// <url>https://paymentexperience.cp.microsoft.com/v7.0/{accountId}/tokensEx/{ntid}/challenges/{challengeid}</url>
        /// <param name="payload" required="true" cref="object" in="body">Payment instrument</param>
        /// <param name="accountId" required="true" cref="string" in="path">Account ID</param>
        /// <param name="ntid" required="true" cref="string" in="path">Network token ID</param>
        /// <param name="challengeId" required="true" cref="string" in="path">ChallengeId ID</param>
        /// <param name="country" required="false" cref="string" in="query">two letter country id</param>
        /// <param name="language" required="false" cref="string" in="query">Language code</param>
        /// <param name="partner" required="false" cref="string" in="query">Partner name</param>
        /// <response code="200">A list of PIDLResource</response>
        /// <returns>A list of PIDLResource object</returns>
        [SuppressMessage("Microsoft.Performance", "CA1822", Justification = "Needs to be an instance method for Route action selection")]
        [HttpPost]
        public async Task<HttpResponseMessage> PostChallenge([FromBody] PIDLData payload, string accountId, string ntid, string challengeId, string country, string language, string partner)
        {
            EventTraceActivity traceActivityId = this.Request.GetRequestCorrelationId();

            PaymentExperienceSetting setting = this.GetPaymentExperienceSetting(V7.Constants.Operations.Get);

            string puid = await this.TryGetClientContext(GlobalConstants.ClientContextKeys.MsaProfile.Puid);
            string emailAddress = payload.TryGetPropertyValueFromPIDLData(AgenticPaymentRequestData.AccountHolderEmail);
            if (string.IsNullOrWhiteSpace(emailAddress))
            {
                emailAddress = await this.TryGetClientContext(GlobalConstants.ClientContextKeys.MsaProfile.EmailAddress);
            }

            string challengeMethodId = payload.TryGetPropertyValueFromPIDLData(AgenticPaymentRequestData.ChallengeMethodId);
            this.ValiateRequestData(challengeMethodId, AgenticPaymentRequestData.ChallengeMethodId);

            // requestChallengeResponse will be used in another PR to set the MaxValidationAttempts the user could try in the PIDL
            RequestChallengeResponse requestChallengeResponse = await this.Settings.NetworkTokenizationServiceAccessor.RequestChallenge(ntid, challengeId, challengeMethodId, puid, traceActivityId, this.ExposedFlightFeatures, emailAddress);

            List<PIDLResource> pidlResources = PIDLResourceFactory.Instance.GetSmsChallengeDescriptionForDeviceBinding(ntid, language, challengeId, challengeMethodId, partner, country, setting, this.ExposedFlightFeatures);

            // Updates default values for PropertyDescriptions in pidlResources based on matching keys in payload
            AgenticPaymentHelper.UpdateDefaultValuesFromPayload(pidlResources, payload);

            PIDLResource smsChallengePidlResource = new PIDLResource()
            {
                ClientAction = new PXCommon.ClientAction(PXCommon.ClientActionType.Pidl)
                {
                    Context = pidlResources,
                }
            };

            return this.Request.CreateResponse(smsChallengePidlResource);
        }

        /// <summary>
        /// Tokens controller
        /// </summary>
        /// <group>Tokens</group>
        /// <verb>Post</verb>
        /// <url>https://paymentexperience.cp.microsoft.com/v7.0/{accountId}/tokensEx/{ntid}/challenges/{challengeid}/validate</url>
        /// <param name="payload" required="true" cref="object" in="body">Payment instrument</param>
        /// <param name="accountId" required="true" cref="string" in="path">Account ID</param>
        /// <param name="ntid" required="true" cref="string" in="path">Network token ID</param>
        /// <param name="challengeId" required="true" cref="string" in="path">ChallengeId ID</param>
        /// <param name="country" required="false" cref="string" in="path">two letter country id</param>
        /// <param name="language" required="false" cref="string" in="query">Language code</param>
        /// <param name="partner" required="false" cref="string" in="query">Partner name</param>
        /// <param name="challengeMethodId" required="false" cref="string" in="query">ChallengeMethod Id</param>
        /// <response code="200">A list of PIDLResource</response>
        /// <returns>A list of PIDLResource object</returns>
        [SuppressMessage("Microsoft.Performance", "CA1822", Justification = "Needs to be an instance method for Route action selection")]
        [HttpPost]
        public async Task<HttpResponseMessage> ValidateChallenge([FromBody] PIDLData payload, string accountId, string ntid, string challengeId, string country, string language, string partner, string challengeMethodId)
        {
            EventTraceActivity traceActivityId = this.Request.GetRequestCorrelationId();

            PaymentExperienceSetting setting = this.GetPaymentExperienceSetting(V7.Constants.Operations.Get);
            string puid = await this.TryGetClientContext(GlobalConstants.ClientContextKeys.MsaProfile.Puid);
            string emailAddress = payload.TryGetPropertyValueFromPIDLData(AgenticPaymentRequestData.AccountHolderEmail);
            if (string.IsNullOrWhiteSpace(emailAddress))
            {
                emailAddress = await this.TryGetClientContext(GlobalConstants.ClientContextKeys.MsaProfile.EmailAddress);
            }

            string otp = payload.TryGetPropertyValueFromPIDLData(AgenticPaymentRequestData.Pin);
            this.ValiateRequestData(otp, AgenticPaymentRequestData.Pin);

            string paymentMethodType = payload.TryGetPropertyValueFromPIDLData(AgenticPaymentRequestData.PaymentMethodType);
            this.ValiateRequestData(paymentMethodType, AgenticPaymentRequestData.PaymentMethodType);

            string piid = payload.TryGetPropertyValueFromPIDLData(AgenticPaymentRequestData.PaymentInstrumentId);
            this.ValiateRequestData(piid, AgenticPaymentRequestData.PaymentInstrumentId);

            await this.Settings.NetworkTokenizationServiceAccessor.ValidateChallenge(ntid, challengeId, challengeMethodId, puid, traceActivityId, this.ExposedFlightFeatures, otp, emailAddress);

            List<PIDLResource> pidlResources = PIDLResourceFactory.Instance.GetPaymentTokenDescriptions(country, paymentMethodType, language, "create", V7.Constants.Operations.Get, partner, this.ExposedFlightFeatures, setting, piid);

            // Updates default values for PropertyDescriptions in pidlResources based on matching keys in payload
            AgenticPaymentHelper.UpdateDefaultValuesFromPayload(pidlResources, payload);

            PIDLResource tokenPidlResource = new PIDLResource()
            {
                ClientAction = new PXCommon.ClientAction(PXCommon.ClientActionType.Pidl)
                {
                    Context = pidlResources,
                }
            };

            return this.Request.CreateResponse(tokenPidlResource);
        }

        /// <summary>
        /// Tokens controller
        /// </summary>
        /// <group>Tokens</group>
        /// <verb>POST</verb>
        /// <url>https://paymentexperience.cp.microsoft.com/v7.0/{accountId}/tokensEx/{ntid}/mandates</url>
        /// <param name="payload" required="true" cref="object" in="body">Payment instrument</param>
        /// <param name="accountId" required="true" cref="string" in="path">Account ID</param>
        /// <param name="ntid" required="true" cref="string" in="path">Network token ID</param>
        /// <response code="200">A list of PIDLResource</response>
        /// <returns>A list of PIDLResource object</returns>
        [SuppressMessage("Microsoft.Performance", "CA1822", Justification = "Needs to be an instance method for Route action selection")]
        [HttpPost]
        public async Task<HttpResponseMessage> Mandates([FromBody] PIDLData payload, string accountId, string ntid)
        {
            EventTraceActivity traceActivityId = this.Request.GetRequestCorrelationId();

            //// TODO: need to finalize if appInstance and assuranceData are needed. if needed, they should be extracted from the payload
            string puid = await this.TryGetClientContext(GlobalConstants.ClientContextKeys.MsaProfile.Puid);
            string deviceId = await this.TryGetClientContext(GlobalConstants.ClientContextKeys.DeviceInfo.DeviceId);
            string emailAddress = await this.TryGetClientContext(GlobalConstants.ClientContextKeys.MsaProfile.EmailAddress);
            string appInstance = null; // Not finalized yet, placeholder for future use
            string assuranceData = payload.TryGetPropertyValueFromPIDLData(AgenticPaymentRequestData.FIDOResponse);
            this.ValiateRequestData(assuranceData, AgenticPaymentRequestData.FIDOResponse);

            var mandatesAndMerchantName = TokensExController.ExtractMandatesAndMerchantName(payload);
            var mandates = mandatesAndMerchantName.Item1;
            var merchantName = mandatesAndMerchantName.Item2;

            PasskeyMandateResponse response = await this.Settings.NetworkTokenizationServiceAccessor.SetMandates(ntid, puid, deviceId, traceActivityId, this.ExposedFlightFeatures, appInstance, assuranceData, mandates);
            return this.Request.CreateResponse(response);
        }             

        /// <summary>
        /// Extracts mandates from payload and returns the merchant name from the first mandate
        /// </summary>
        /// <param name="payload">PIDLData containing the mandates property</param>
        /// <returns>A tuple containing the parsed mandates list and merchant name from the first mandate</returns>
        private static Tuple<List<Mandate>, string> ExtractMandatesAndMerchantName(PIDLData payload)
        {
            List<Mandate> mandates = null;
            string merchantName = null;

            if (payload != null)
            {
                string mandatesJson = payload.TryGetPropertyValueFromPIDLData("mandates");
                if (!string.IsNullOrEmpty(mandatesJson))
                {
                    try
                    {
                        mandates = JsonConvert.DeserializeObject<List<Mandate>>(mandatesJson);
                    }
                    catch (JsonException)
                    {
                        // Handle JSON parsing error - mandates will remain null
                        mandates = null;
                    }
                }

                // Extract merchantName from the first mandate if available
                if (mandates != null && mandates.Count > 0)
                {
                    merchantName = mandates[0].MerchantName;
                }
            }

            return Tuple.Create(mandates, merchantName);
        }

        /// <summary>
        /// Creates a client action for passkey setup with post message functionality
        /// </summary>
        /// <param name="token">The network token metadata response</param>
        /// <param name="passkeySetupResponse">The passkey setup operation response</param>
        /// <returns>A ClientAction configured for post message to child iframes</returns>
        private static PXCommon.ClientAction CreatePasskeySetupClientAction(GetTokenMetadataResponse token, PasskeyOperationResponse passkeySetupResponse)
        {
            var clientActionAfterAuthenticate = new PXCommon.ClientAction(PXCommon.ClientActionType.UpdatePropertyValue)
            {
                Context = new UpdatePropertyValueActionContext()
                {
                    PropertyName = "FIDOResponse",
                    PropertyValue = string.Empty  // Will be filled by the script
                },
                NextAction = new PXCommon.ClientAction(PXCommon.ClientActionType.InvokePidlAction)
                {
                    Context = new RestLink()
                    {
                        Method = "POST",
                        Href = $"https://{{pifd-endpoint}}/users/{{userId}}/tokensEx/{token.NetworkTokenId}/mandates"
                    }
                }
            };

            var authenticateCommand = new AuthenticateCommand()
            {
                RequestID = string.Empty, // Will be filled by the script
                AuthenticationContext = passkeySetupResponse.AuthContext,
                AuthenticationType = "AUTHENTICATE",
                Version = "1",
                ContentType = "application/json",
            };

            var message = new Dictionary<string, object>()
            {
                { "clientActionAfterAuthenticate", clientActionAfterAuthenticate },
                { "authenticateCommand", authenticateCommand }
            };

            var postMessageContext = new PostMessageToChildIFramesActionContext()
            {
                Message = message,
                TargetOrigin = "*"
            };

            var displayHintAction = new DisplayHintAction(DisplayHintActionType.postMessageToChildIFrames.ToString())
            {
                Context = postMessageContext
            };

            return new PXCommon.ClientAction(PXCommon.ClientActionType.InvokePidlAction)
            {
                Context = displayHintAction
            };
        }

        private async Task<HttpResponseMessage> HandleDeviceBindingAndPasskeySetup(
           GetTokenMetadataResponse token,
           PaymentInstrument paymentInstrument,
           EventTraceActivity traceActivityId,
           List<string> exposedFlightFeatures,
           string emailAddress,
           string puid,
           string deviceId,
           string language,
           string partner,
           PaymentExperienceSetting setting,
           int authenticationAmount,
           string currencyCode,
           string sessionContext,
           string browserData,
           string applicationUrl,
           string merchantName,
           string country,
           PIDLData payload)
        {
            PXCommon.ClientAction pidlClientAction = null;

            PasskeyOperationResponse passkeyAuthenticateResponse = await this.Settings.NetworkTokenizationServiceAccessor.PasskeyAuthenticate(
                token.NetworkTokenId,
                authenticationAmount,
                currencyCode,
                puid,
                deviceId,
                traceActivityId,
                exposedFlightFeatures,
                sessionContext,
                browserData,
                applicationUrl,
                merchantName);

            if (passkeyAuthenticateResponse.Action == PasskeyAction.REGISTER_DEVICE_BINDING)
            {
                RequestDeviceBindingResponse deviceBindingResponse = await this.Settings.NetworkTokenizationServiceAccessor.RequestDeviceBinding(token.NetworkTokenId, puid, deviceId, traceActivityId, ExposedFlightFeatures, token.ExternalCardReference, emailAddress, sessionContext, browserData);

                if (deviceBindingResponse.Status == ChallengeStatus.Challenge)
                {
                    List<PIDLResource> challengeMethodTypesPidl = PIDLResourceFactory.Instance.GetStaticPidlDescriptions(V7.Constants.PidlResourceDescriptionType.TokensChallengeTypesPidl, language, partner, setting: setting);

                    foreach (PIDLResource pidlResource in challengeMethodTypesPidl)
                    {
                        PropertyDescription challengeTypeProperty = pidlResource.GetPropertyDescriptionByPropertyName("challengeMethodId");
                        PropertyDisplayHint challengeTypeDisplayHint = pidlResource.GetDisplayHintById("tokensChallengeTypesSelectChallengeType") as PropertyDisplayHint;
                        if (challengeTypeProperty != null && deviceBindingResponse.ChallengeMethods != null && challengeTypeDisplayHint != null)
                        {
                            // Add each challenge method from the response to the possible values
                            challengeTypeProperty.PossibleValues = challengeTypeProperty.PossibleValues == null ? new Dictionary<string, string>() : challengeTypeProperty.PossibleValues;
                            challengeTypeDisplayHint.PossibleValues = challengeTypeDisplayHint.PossibleValues == null ? new Dictionary<string, string>() : challengeTypeDisplayHint.PossibleValues;
                            challengeTypeDisplayHint.PossibleOptions = challengeTypeDisplayHint.PossibleOptions == null ? new Dictionary<string, SelectOptionDescription>() : challengeTypeDisplayHint.PossibleOptions;
                            foreach (ChallengeMethod challengeMethod in deviceBindingResponse.ChallengeMethods)
                            {
                                challengeTypeProperty.PossibleValues[challengeMethod.ChallengeMethodId] = challengeMethod.ChallengeValue;
                                challengeTypeDisplayHint.PossibleValues[challengeMethod.ChallengeMethodId] = challengeMethod.ChallengeValue;
                                challengeTypeDisplayHint.PossibleOptions[challengeMethod.ChallengeMethodId] = new SelectOptionDescription
                                {
                                    DisplayText = challengeMethod.ChallengeValue
                                };
                            }
                        }

                        ButtonDisplayHint buttonDisplayHint = pidlResource.GetDisplayHintById("saveNextButton") as ButtonDisplayHint;
                        if (buttonDisplayHint != null)
                        {
                            buttonDisplayHint.Action = new DisplayHintAction(DisplayHintActionType.submit.ToString())
                            {
                                Context = new RestLink()
                                {
                                    Method = "POST",
                                    Href = $"https://{{pifd-endpoint}}/users/{{userId}}/tokensEx/{token.NetworkTokenId}/challenges/{deviceBindingResponse.ChallengeId}?country={country}&language={language}&partner={partner}"
                                }
                            };
                        }
                    }

                    // Updates default values for PropertyDescriptions in challengeMethodTypesPidl based on matching keys in payload
                    AgenticPaymentHelper.UpdateDefaultValuesFromPayload(challengeMethodTypesPidl, payload);

                    pidlClientAction = new PXCommon.ClientAction(PXCommon.ClientActionType.Pidl)
                    {
                        Context = challengeMethodTypesPidl
                    };
                }
                else if (deviceBindingResponse.Status == ChallengeStatus.Approved)
                {
                    PasskeyOperationResponse passkeySetupResponse = await this.Settings.NetworkTokenizationServiceAccessor.PasskeySetup(
                        token.NetworkTokenId,
                        authenticationAmount,
                        currencyCode,
                        puid,
                        deviceId,
                        traceActivityId,
                        exposedFlightFeatures,
                        sessionContext,
                        browserData,
                        applicationUrl,
                        merchantName);

                    pidlClientAction = CreatePasskeySetupClientAction(token, passkeySetupResponse);
                }
                else
                {
                    return this.Request.CreateResponse(HttpStatusCode.BadRequest, "No valid status found in deviceBindingResponse.");
                }

                PIDLResource challengePidlResource = new PIDLResource()
                {
                    ClientAction = pidlClientAction,
                };

                return this.Request.CreateResponse(challengePidlResource);
            }
            else if (passkeyAuthenticateResponse.Action == PasskeyAction.AUTHENTICATE)
            {
                PIDLResource validatedPidlResource = new PIDLResource()
                {
                    ClientAction = CreatePasskeySetupClientAction(token, passkeyAuthenticateResponse),
                };

                return this.Request.CreateResponse(validatedPidlResource);
            }
            else
            {
                return this.Request.CreateResponse(HttpStatusCode.BadRequest, "No valid action found in passkeyAuthenticateResponse.");
            }
        }

        private void ValiateRequestData(string requestData, string dataName)
        {
            if (string.IsNullOrEmpty(requestData))
            {
                this.CreateInvalidRequestDataError(dataName);
            }
        }

        private void ValiateRequestData(int requestData, string dataName)
        {
            if (requestData <= 0)
            {
                this.CreateInvalidRequestDataError(dataName);
            }
        }

        private IActionResult CreateInvalidRequestDataError(string dataName)
        {
            var error = new ErrorMessage
            {
                ErrorCode = ErrorCode.InvalidRequestData.ToString(),
                Message = "TokensEx: Missing request data " + dataName,
                Retryable = false,
            };

            return new BadRequestObjectResult(error);
        }
    }
}