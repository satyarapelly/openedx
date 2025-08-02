// <copyright file="AddressesController.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.V7
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using Common.Tracing;
    using Common.Web;
    using Microsoft.Commerce.Payments.PartnerSettingsModel;
    using Microsoft.Commerce.Payments.PidlFactory.V7;
    using Microsoft.Commerce.Payments.PidlFactory.V7.PIDLFeatureProcess;
    using Microsoft.Commerce.Payments.PidlModel.V7;
    using Microsoft.Commerce.Payments.PimsModel.V4;
    using PXCommon;
    using PXService.Model.AccountService.AddressValidation;

    public class AddressesController : ProxyController
    {
        /// <summary>
        /// Legacy Validate address ("/LegacyValidate" is not in the real path, just as a workaround to show multiple APIs with the same path and http verb but with different params in open API doc)
        /// </summary>
        /// <group>Addresses</group>
        /// <verb>POST</verb>
        /// <url>https://paymentexperience.cp.microsoft.com/v7.0/Addresses/LegacyValidate</url>
        /// <param name="address" required="true" cref="object" in="body">address body</param>
        /// <param name="type" required="false" cref="string" in="query">type name</param>
        /// <response code="200">A validation result</response>
        /// <returns>A validation result</returns>
        [HttpPost]
        public async Task<HttpResponseMessage> LegacyValidate([FromBody]PIDLData address, string type = null)
        {
            const string ValidAddressResponse = "Valid";
            const string VerifiedAddressStatus = "Verified";

            EventTraceActivity traceActivityId = this.Request.GetRequestCorrelationId();

            if (string.Equals(type, Constants.AddressTypes.HapiV1SoldToOrganization, System.StringComparison.OrdinalIgnoreCase)
                || string.Equals(type, Constants.AddressTypes.HapiV1SoldToOrganizationCSP, System.StringComparison.OrdinalIgnoreCase)
                || string.Equals(type, Constants.AddressTypes.HapiV1ShipToOrganization, System.StringComparison.OrdinalIgnoreCase)
                || string.Equals(type, Constants.AddressTypes.HapiV1BillToOrganization, System.StringComparison.OrdinalIgnoreCase)
                || string.Equals(type, Constants.AddressTypes.HapiV1SoldToIndividual, System.StringComparison.OrdinalIgnoreCase)
                || string.Equals(type, Constants.AddressTypes.HapiV1ShipToIndividual, System.StringComparison.OrdinalIgnoreCase)
                || string.Equals(type, Constants.AddressTypes.HapiV1BillToIndividual, System.StringComparison.OrdinalIgnoreCase)
                || string.Equals(type, Constants.AddressTypes.HapiV1, System.StringComparison.OrdinalIgnoreCase))
            {
                address.RenameProperty(Constants.HapiV1ModernAccountAddressDataDescriptionPropertyNames.AddressLine1, Constants.LegacyAVSPropertyNames.AddressLine1);
                address.RenameProperty(Constants.HapiV1ModernAccountAddressDataDescriptionPropertyNames.AddressLine2, Constants.LegacyAVSPropertyNames.AddressLine2);
                address.RenameProperty(Constants.HapiV1ModernAccountAddressDataDescriptionPropertyNames.AddressLine3, Constants.LegacyAVSPropertyNames.AddressLine3);
                address.RenameProperty(Constants.HapiV1ModernAccountAddressDataDescriptionPropertyNames.PostalCode, Constants.LegacyAVSPropertyNames.PostalCode);
            }

            object result;
            try
            {
                result = await this.Settings.AccountServiceAccessor.LegacyValidateAddress(address, traceActivityId);

                // Appending original request to the response of a valid address so that PIDL SDK partners 
                // can receive the collected address as part of submit.
                if (string.Equals((string)result, ValidAddressResponse, System.StringComparison.OrdinalIgnoreCase))
                {
                    var response = new ValidateAddressResponse()
                    {
                        OriginalAddress = address,
                        Status = VerifiedAddressStatus
                    };

                    return this.Request.CreateResponse(response);
                }
            }
            catch (ServiceErrorResponseException ex)
            {
                return this.Request.CreateResponse(ex.Response.StatusCode, ex.Error);
            }

            return this.Request.CreateResponse(result);
        }

        /// <summary>
        /// Modern Validate address ("/ModernValidate" is not in the real path, just as a workaround to show multiple APIs with the same path and http verb but with different params in open API doc)
        /// </summary>
        /// <group>Addresses</group>
        /// <verb>POST</verb>
        /// <url>https://paymentexperience.cp.microsoft.com/v7.0/Addresses/ModernValidate</url>
        /// <param name="address" required="true" cref="object" in="body">address body</param>
        /// <response code="200">A validation result</response>
        /// <returns>A validation result</returns>
        [HttpPost]
        public async Task<HttpResponseMessage> ModernValidate([FromBody]PIDLData address)
        {
            EventTraceActivity traceActivityId = this.Request.GetRequestCorrelationId();

            AddressValidationAVSResponse validationResult;
            try
            {
                // Check if the flight PXEnableSecondaryValidationMode is enabled
                if (this.ExposedFlightFeatures != null && this.ExposedFlightFeatures.Contains(Flighting.Features.PXEnableSecondaryValidationMode, StringComparer.OrdinalIgnoreCase))
                {
                    // If the address contain the property "validation_mode", add secondary validation mode property with the value "LegacyBusiness".
                    if (address.ContainsProperty(Constants.PropertyDescriptionIds.ValidationMode))
                    {
                        // If the address contains the property "secondary_validation_mode", set its value to "LegacyBusiness"
                        if (address.ContainsProperty(Constants.PropertyDescriptionIds.SecondaryValidationMode))
                        {
                            address.TrySetProperty(Constants.PropertyDescriptionIds.SecondaryValidationMode, Constants.DisplayPropertyName.LegacyBusiness);
                        }
                        else
                        {
                            // If the address does not contain the property "secondary_validation_mode", add it with the value "LegacyBusiness"
                            address.Add(Constants.PropertyDescriptionIds.SecondaryValidationMode, Constants.DisplayPropertyName.LegacyBusiness);
                        }
                    }
                }

                validationResult = await this.ModernValidateAddress(address, traceActivityId);
            }
            catch (ServiceErrorResponseException ex)
            {
                return this.Request.CreateResponse(ex.Response.StatusCode, ex.Error);
            }

            return this.Request.CreateResponse(validationResult);
        }

        /// <summary>
        /// Modern Validate address ("/ModernValidateByType" is not in the real path, just as a workaround to show multiple APIs with the same path and http verb but with different params in open API doc)
        /// </summary>
        /// <group>Addresses</group>
        /// <verb>POST</verb>
        /// <url>https://paymentexperience.cp.microsoft.com/v7.0/Addresses/ModernValidateByType</url>
        /// <param name="address" required="true" cref="object" in="body">address body</param>
        /// <param name="partner" required="false" cref="string" in="query">Partner name</param>
        /// <param name="language" required="false" cref="string" in="query">Language code</param>
        /// <param name="scenario" required="false" cref="string" in="query">scenario name</param>
        /// <param name="type" required="true" cref="string" in="query">address type</param>
        /// <param name="country" required="true" cref="string" in="query">Two letter country code</param>
        /// <response code="200">A validation result</response>
        /// <returns>A validation result</returns>
        [HttpPost]
        public async Task<HttpResponseMessage> ModernValidate(
            [FromBody] PIDLData address,
            string partner,
            string language,
            string scenario,
            string type,
            string country)
        {
            EventTraceActivity traceActivityId = this.Request.GetRequestCorrelationId();
            PaymentExperienceSetting setting = this.GetPaymentExperienceSetting(Constants.Operations.ValidateInstance);
            this.EnableFlightingsInPartnerSetting(setting, country);
            if (string.Equals(type, Constants.AddressTypes.HapiV1SoldToOrganization, System.StringComparison.OrdinalIgnoreCase)
                || string.Equals(type, Constants.AddressTypes.HapiV1SoldToOrganizationCSP, System.StringComparison.OrdinalIgnoreCase)
                || string.Equals(type, Constants.AddressTypes.HapiV1ShipToOrganization, System.StringComparison.OrdinalIgnoreCase)
                || string.Equals(type, Constants.AddressTypes.HapiV1BillToOrganization, System.StringComparison.OrdinalIgnoreCase)
                || string.Equals(type, Constants.AddressTypes.HapiV1SoldToIndividual, System.StringComparison.OrdinalIgnoreCase)
                || string.Equals(type, Constants.AddressTypes.HapiV1ShipToIndividual, System.StringComparison.OrdinalIgnoreCase)
                || string.Equals(type, Constants.AddressTypes.HapiV1BillToIndividual, System.StringComparison.OrdinalIgnoreCase)
                || string.Equals(type, Constants.AddressTypes.HapiV1, System.StringComparison.OrdinalIgnoreCase))
            {
                address.RenameProperty(Constants.HapiV1ModernAccountAddressDataDescriptionPropertyNames.AddressLine1, Constants.LegacyAVSPropertyNames.AddressLine1);
                address.RenameProperty(Constants.HapiV1ModernAccountAddressDataDescriptionPropertyNames.AddressLine2, Constants.LegacyAVSPropertyNames.AddressLine2);
                address.RenameProperty(Constants.HapiV1ModernAccountAddressDataDescriptionPropertyNames.AddressLine3, Constants.LegacyAVSPropertyNames.AddressLine3);
                address.RenameProperty(Constants.HapiV1ModernAccountAddressDataDescriptionPropertyNames.PostalCode, Constants.LegacyAVSPropertyNames.PostalCode);
            }

            if (string.Equals(type, Constants.AddressTypes.HapiServiceUsageAddress, System.StringComparison.OrdinalIgnoreCase))
            {
                address.RenameProperty(Constants.HapiServiceUsageAddressPropertyNames.AddressLine1, Constants.LegacyAVSPropertyNames.AddressLine1);
                address.RenameProperty(Constants.HapiServiceUsageAddressPropertyNames.AddressLine2, Constants.LegacyAVSPropertyNames.AddressLine2);
                address.RenameProperty(Constants.HapiServiceUsageAddressPropertyNames.AddressLine3, Constants.LegacyAVSPropertyNames.AddressLine3);
                address.RenameProperty(Constants.HapiServiceUsageAddressPropertyNames.PostalCode, Constants.LegacyAVSPropertyNames.PostalCode);
                address.RenameProperty(Constants.HapiServiceUsageAddressPropertyNames.Country, Constants.LegacyAVSPropertyNames.Country);
                address.RenameProperty(Constants.HapiServiceUsageAddressPropertyNames.Region, Constants.LegacyAVSPropertyNames.Region);
            }

            this.Request.AddPartnerProperty(partner?.ToLower());

            HttpResponseMessage response;

            PXAddressV3Info userEnteredAddress = new PXAddressV3Info(address);
            bool usePidlPage = !string.Equals(scenario, Constants.ScenarioNames.SuggestAddressesTradeAVSUsePidlModal, StringComparison.OrdinalIgnoreCase);
            bool useV2UXForPidlPage = string.Equals(scenario, Constants.ScenarioNames.SuggestAddressesTradeAVSUsePidlPageV2, StringComparison.OrdinalIgnoreCase);

            // Do legacy validation before modern validation for us and ca
            object result;
            if (Constants.CountriesToDoLegacyValidationBeforeModernValidationAVS.Contains(country))
            {
                try
                {
                    // Use modern validate in legacy business mode for SuggestAddressesTradeAVSUsePidlPageV2 scenario
                    if (useV2UXForPidlPage)
                    {
                        if (address.ContainsProperty("validation_mode"))
                        {
                            address.TrySetProperty("validation_mode", "LegacyBusiness");
                        }
                        else
                        {
                            address.Add("validation_mode", "LegacyBusiness");
                        }

                        result = await this.ModernValidateAddress(address, traceActivityId);
                        address.Remove("validation_mode");
                    }
                    else
                    {
                        result = await this.Settings.AccountServiceAccessor.LegacyValidateAddress(address, traceActivityId);
                    }
                }
                catch (ServiceErrorResponseException ex)
                {
                    return this.Request.CreateResponse(ex.Response.StatusCode, ex.Error);
                }
            }

            try
            {
                // Check if the flight PXEnableSecondaryValidationMode is enabled
                if (this.ExposedFlightFeatures != null && this.ExposedFlightFeatures.Contains(Flighting.Features.PXEnableSecondaryValidationMode, StringComparer.OrdinalIgnoreCase))
                {
                    // If the address contain the property "validation_mode", add secondary validation mode property with the value "LegacyBusiness".
                    if (address.ContainsProperty(Constants.PropertyDescriptionIds.ValidationMode))
                    {
                        // If the address contains the property "secondary_validation_mode", set its value to "LegacyBusiness"
                        if (address.ContainsProperty(Constants.PropertyDescriptionIds.SecondaryValidationMode))
                        {
                            address.TrySetProperty(Constants.PropertyDescriptionIds.SecondaryValidationMode, Constants.DisplayPropertyName.LegacyBusiness);
                        }
                        else
                        {
                            // If the address does not contain the property "secondary_validation_mode", add it with the value "LegacyBusiness"
                            address.Add(Constants.PropertyDescriptionIds.SecondaryValidationMode, Constants.DisplayPropertyName.LegacyBusiness);
                        }

                        result = await this.ModernValidateAddress(address, traceActivityId);
                    }
                }

                response = await this.Suggest(userEnteredAddress, partner, language, traceActivityId, type, usePidlPage, useV2UXForPidlPage, Constants.ScenarioNames.SuggestAddressesTradeAVS, setting, country);
            }
            catch (ServiceErrorResponseException ex)
            {
                return this.Request.CreateResponse(ex.Response.StatusCode, ex.Error, "application/json");
            }

            return response;
        }

        private async Task<AddressValidationAVSResponse> ModernValidateAddress(PIDLData address, EventTraceActivity traceActivityId)
        {
            AddressValidationAVSResponse validationResult;
            validationResult = await this.Settings.AccountServiceAccessor.ModernValidateAddress<AddressValidationAVSResponse>(address, traceActivityId);

            if (validationResult.Status != AddressAVSValidationStatus.Verified
            && validationResult.Status != AddressAVSValidationStatus.VerifiedShippable)
            {
                if (validationResult.ValidationMessage != null)
                {
                    foreach (string key in GlobalConstants.AvsErrorMessages.Keys)
                    {
                        if (validationResult.ValidationMessage.Contains(key))
                        {
                            validationResult.Status = GlobalConstants.AvsErrorMessages[key];
                            break;
                        }
                    }
                }

                var innerError = new ServiceErrorResponse(validationResult.Status.ToString(), validationResult.ValidationMessage, PXCommon.Constants.ServiceNames.AddressEnrichmentService);
                var error = new ServiceErrorResponse(traceActivityId.ActivityId.ToString(), GlobalConstants.ServiceName, innerError);
                var exception = new ServiceErrorResponseException() { Error = error, Response = this.Request.CreateResponse(HttpStatusCode.BadRequest, validationResult) };

                throw TraceCore.TraceException(traceActivityId, exception);
            }
            else
            {
                // Override original_address to keep submitted payload with non-address properties (first name, last name, email, phone etc.)
                // modern AVS returns only address related properties (address_line1, city, country etc.) as original_address
                validationResult.OriginalAddress = address;
            }

            return validationResult;
        }

        private async Task<HttpResponseMessage> Suggest(PXAddressV3Info userEnteredAddress, string partner, string language, EventTraceActivity traceActivityId, string addressType, bool usePidlPage, bool useV2UXForPidlPage, string scenario, PaymentExperienceSetting setting, string country)
        {
            userEnteredAddress.Id = PidlFactory.GlobalConstants.SuggestedAddressesIds.UserEntered;
            var operation = Constants.Operations.ValidateInstance;
            var avsAccessor = this.Settings.AddressEnrichmentServiceAccessor;

            List<PIDLResource> pidls = new List<PIDLResource>();
           
            pidls = await AddressesExHelper.SuggestAddressTradeAvsPidl(
                userEnteredAddress,
                language,
                partner,
                operation,
                traceActivityId,
                this.ExposedFlightFeatures,
                this.Settings.AccountServiceAccessor,
                avsAccessor,
                scenario,
                addressType,
                usePidlPage,
                useV2UXForPidlPage,
                setting);

            FeatureContext featureContext = new FeatureContext(
                country,
                GetSettingTemplate(partner, setting, Constants.DescriptionTypes.AddressDescription, null, null),
                Constants.DescriptionTypes.AddressDescription,
                operation,
                scenario,
                language,
                null,
                this.ExposedFlightFeatures,
                setting?.Features,
                null,
                null,
                smdMarkets: null,
                originalPartner: partner,
                originalTypeName: addressType,
                isGuestAccount: GuestAccountHelper.IsGuestAccount(this.Request));

            PostProcessor.Process(pidls, PIDLResourceFactory.FeatureFactory, featureContext);

            // set isSubmitGroup to false for groups containing buttons in the address suggestion page in TradeAVS V1, so that when partner set showSubmitBlock to false, the buttons from address suggestion page won't be hidden 
            foreach (PIDLResource pidl in pidls)
            {
                if (this.ExposedFlightFeatures?.Contains(Flighting.Features.PXSetIsSubmitGroupFalseForTradeAVSV1, StringComparer.OrdinalIgnoreCase) ?? false
                    && string.Equals(pidl.DisplayPages?.First()?.DisplayName, Constants.DisplayHintIds.AddressSuggestionTradePage, StringComparison.OrdinalIgnoreCase))
                {
                    // edit button
                    FeatureHelper.SetIsSubmitGroupFalse(pidl, Constants.DisplayHintIds.AddressChangeTradeAVSGroup);

                    // When there are one or multiple suggestions, addressUseButton: Use this address
                    FeatureHelper.SetIsSubmitGroupFalse(pidl, Constants.DisplayHintIds.AddressUseCloseGroup);

                    // When there is no address suggestion, userEnteredButton: Use this address
                    FeatureHelper.SetIsSubmitGroupFalse(pidl, Constants.DisplayHintIds.AddressUseEnteredGroup);
                }
            }

            if (pidls.Count > 0 && pidls[0]?.ClientAction != null)
            {
                if (pidls[0].ClientAction.ActionType == ClientActionType.None)
                {
                    return this.Request.CreateResponse(pidls[0].ClientAction.Context);
                }
                else if (pidls[0].ClientAction.ActionType == ClientActionType.MergeData)
                {
                    return this.Request.CreateResponse(pidls[0]);
                }
            }

            ClientActionType actionType = ClientActionType.PidlModal;

            // if usePidlPage, then it would need three changes
            // 1. clientAction needs to be changed to PidlPage
            // 2. modalGroupDiplayHint.IsModalGroup needs to be set to be false
            // 3. all closeModalDialog pidlActions need to be changed to closePidlPage pidlAction
            if (usePidlPage)
            {
                actionType = ClientActionType.PidlPage;
                foreach (PIDLResource pidl in pidls)
                {
                    foreach (string modalGroupHintId in Constants.ModalGroupIds)
                    {
                        ContainerDisplayHint modalGroupDiplayHint = pidl.GetPidlContainerDisplayHintbyDisplayId(modalGroupHintId);
                        if (modalGroupDiplayHint != null)
                        {
                            modalGroupDiplayHint.IsModalGroup = false;
                        }
                    }

                    DisplayHint addressChangeTradeAVSButtonDisplayHint = pidl.GetDisplayHintById(useV2UXForPidlPage ? Constants.DisplayHintIds.AddressChangeTradeAVSV2Button : Constants.DisplayHintIds.AddressChangeTradeAVSButton);
                    if (addressChangeTradeAVSButtonDisplayHint != null)
                    {
                        addressChangeTradeAVSButtonDisplayHint.Action.ActionType = DisplayHintActionType.closePidlPage.ToString();
                    }
                }
            }
                
            ClientAction clientAction = new ClientAction(actionType, pidls);

            PIDLResource suggestedAddressPidl = new PIDLResource();
            suggestedAddressPidl.ClientAction = clientAction;

            return this.Request.CreateResponse(suggestedAddressPidl);
        }
    }
}
