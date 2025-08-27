// <copyright file="AddressesExHelper.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.V7
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Azure;
    using Microsoft.Commerce.Payments.Common.Tracing;
    using Microsoft.Commerce.Payments.Common.Web;
    using Microsoft.Commerce.Payments.PartnerSettingsModel;
    using Microsoft.Commerce.Payments.PidlFactory.V7;
    using Microsoft.Commerce.Payments.PidlModel.V7;
    using Microsoft.Commerce.Payments.PimsModel.V4;
    using Microsoft.Commerce.Payments.PXCommon;
    using Microsoft.Commerce.Payments.PXService.Accessors.AddressEnrichmentService.DataModel;
    using Microsoft.Commerce.Payments.PXService.AddressEnrichmentService.V7;
    using Microsoft.Commerce.Tracing;
    using Microsoft.Diagnostics.Tracing.Etlx;
    using Newtonsoft.Json;
    using static Microsoft.Commerce.Payments.PXService.GlobalConstants;
    using AddressEnrichmentServiceConstants = Microsoft.Commerce.Payments.PXService.AddressEnrichmentService.V7.Constants;
    using EventLevel = Diagnostics.Tracing.EventLevel;

    public static class AddressesExHelper
    {
        public static async Task<List<PIDLResource>> SuggestAddressPidl(
            string accountId,
            PXAddressV3Info address,
            string language,
            string partner,
            string operation,
            EventTraceActivity traceActivityId,
            bool setAsDefaultBilling,
            List<string> exposedFlightFeatures,
            IAccountServiceAccessor accountServiceAccessor,
            IAddressEnrichmentServiceAccessor addressEnrichmentServiceAccessor,
            bool existingAddress = false,
            string scenario = null,
            bool useValidateInstanceV2UXForPidlPage = false,
            PaymentExperienceSetting setting = null)
        {
            List<PIDLResource> pidlDoc = new List<PIDLResource>()
            {
                new PIDLResource()
                {
                    ClientAction = new ClientAction(ClientActionType.ReturnContext, address)
                }
            };

            // Case 1 : If already compliant or not applicable with zip+4 add address and return
            if ((address.IsCustomerConsented ?? false) || (address.IsZipPlus4Present ?? false) || !string.Equals(address.Country, "us", StringComparison.InvariantCultureIgnoreCase))
            {
                if (!existingAddress)
                {
                    pidlDoc = await PostAddress(
                        accountServiceAccessor,
                        accountId,
                        address,
                        GlobalConstants.AccountServiceApiVersion.V3,
                        V7.Constants.SyncToLegacyCodes.NoSyncAndValidation,
                        traceActivityId,
                        setAsDefaultBilling,
                        exposedFlightFeatures);
                }

                return pidlDoc;
            }

            Address addressEnrichment = new Address(address);

            // Run lightweight legacy validate when setAsDefaultBilling = true and add address flow.
            if (setAsDefaultBilling && !existingAddress)
            {
                var result = await accountServiceAccessor.LegacyValidateAddress(address, traceActivityId);
            }

            AddressLookupResponse lookupResponse;
            try
            {
                lookupResponse = await ValidateAddressWrapper(accountServiceAccessor, addressEnrichmentServiceAccessor, addressEnrichment, traceActivityId, exposedFlightFeatures);
            }
            catch (Exception ex)
            {
                // If there is any address enrichment service exception, fall back to the user entered address 
                SllWebLogger.TracePXServiceException($"SuggestAddressAvsPidl validateAddress Wrapper throw an exception: {ex.ToString()}", traceActivityId);
                lookupResponse = null;
            }

            if (lookupResponse?.Status == EnrichmentValidationStatus.Verified ||
                lookupResponse?.Status == EnrichmentValidationStatus.VerifiedShippable)
            {
                // Case 2 : If verified or verifiedshippable
                // 1. compare with existing address,
                // a. if same, directly return address object,
                // b. otherwise post address and return new address object
                // since the code is stub only, we directly return pidlDoc here and will add above logic later
                SuggestedAddress suggestedAddress = lookupResponse?.SuggestedAddresses?.FirstOrDefault();
                if (suggestedAddress?.Address?.PostalCode?.Length == 10)
                {
                    AddressInfoV3 address9digitZip = lookupResponse?.SuggestedAddresses?.FirstOrDefault()?.ToAddressInfoV3(address);
                    if (address9digitZip != null)
                    {
                        // to fix known issue, lookup API set addressline_1 to null for PO BOX
                        if (string.IsNullOrEmpty(address9digitZip.AddressLine1))
                        {
                            address.PostalCode = address9digitZip.PostalCode;
                            address9digitZip = address;
                        }

                        try
                        {
                            PXAddressV3Info verifiedAddress = new PXAddressV3Info(address9digitZip);
                            pidlDoc = await PostAddress(
                                accountServiceAccessor,
                                accountId,
                                verifiedAddress,
                                GlobalConstants.AccountServiceApiVersion.V3,
                                V7.Constants.SyncToLegacyCodes.NoSyncAndValidation,
                                traceActivityId,
                                setAsDefaultBilling,
                                exposedFlightFeatures);
                        }
                        catch (Exception exception)
                        {
                            // if post address with setAsDefaultBilling, then fail back to avsStatus == none.
                            SllWebLogger.TracePXServiceException($"zip4 {exception?.ToString()}", traceActivityId);
                            PXAddressV3Info userPickedAddress = new PXAddressV3Info(address);
                            return AddressSelectionHelper.GetSuggestedAddressesPidls(
                                new List<PXAddressV3Info> { userPickedAddress },
                                userPickedAddress,
                                partner,
                                language,
                                operation,
                                lookupResponse.Status.ToString(),
                                exposedFlightFeatures,
                                setAsDefaultBilling: setAsDefaultBilling,
                                scenario: scenario);
                        }
                    }

                    return pidlDoc;
                }
            }
            else if (lookupResponse?.Status == EnrichmentValidationStatus.InteractionRequired ||
                lookupResponse?.Status == EnrichmentValidationStatus.PremisesPartial ||
                lookupResponse?.Status == EnrichmentValidationStatus.StreetPartial)
            {
                // Case 3: InteractionRequired: Suggested Pages should be shown.
                // there are a couple of features need to be supported here.
                List<PXAddressV3Info> suggestedAddresses = lookupResponse?.SuggestedAddresses
                    ?.Select((suggestAddress, index) => suggestAddress.ToPXAddressV3Info(index.ToString(), address))
                    .ToList();

                // Make sure we have a least an empty list so we don't have to check for null
                if (suggestedAddresses != null && suggestedAddresses.Count > 0)
                {
                    return AddressSelectionHelper.GetSuggestedAddressesPidls(
                        suggestedAddresses,
                        address,
                        partner,
                        language,
                        operation,
                        lookupResponse.Status.ToString(),
                        exposedFlightFeatures,
                        setAsDefaultBilling: setAsDefaultBilling,
                        scenario: scenario,
                        useValidateInstanceV2UXForPidlPage: useValidateInstanceV2UXForPidlPage,
                        setting: setting);
                }
            }
            else if (lookupResponse?.Status == EnrichmentValidationStatus.None ||
               lookupResponse?.Status == EnrichmentValidationStatus.NotValidated ||
               lookupResponse?.Status == EnrichmentValidationStatus.Multiple)
            {
                // Case 4: NotValidated or None or Multiple (per AVS discussion): Avs was not able to validated the address
                //         Return pidl with only the address entered by the users
                PXAddressV3Info userPickedAddress = new PXAddressV3Info(address);
                return AddressSelectionHelper.GetSuggestedAddressesPidls(
                    new List<PXAddressV3Info> { userPickedAddress },
                    userPickedAddress,
                    partner,
                    language,
                    operation,
                    lookupResponse.Status.ToString(),
                    exposedFlightFeatures,
                    setAsDefaultBilling: setAsDefaultBilling,
                    scenario: scenario,
                    useValidateInstanceV2UXForPidlPage: useValidateInstanceV2UXForPidlPage,
                    setting: setting);
            }

            if (!existingAddress)
            {
                SllLogger.TraceMessage($"SuggestAddressPidl Unknown AVS Status: {lookupResponse?.Status.ToString() ?? "empty avs response"}. activityID {traceActivityId?.ActivityId.ToString() ?? "empty traceActivityId"}. cv {traceActivityId?.CorrelationVectorV4?.ToString() ?? "empty cv"}", EventLevel.Informational);
                return await PostAddress(
                    accountServiceAccessor,
                    accountId,
                    address,
                    GlobalConstants.AccountServiceApiVersion.V3,
                    V7.Constants.SyncToLegacyCodes.NoSyncAndValidation,
                    traceActivityId,
                    setAsDefaultBilling,
                    exposedFlightFeatures);
            }

            return pidlDoc;
        }

        public static async Task<List<PIDLResource>> SuggestAddressTradeAvsPidl(
            PXAddressV3Info address,
            string language,
            string partner,
            string operation,
            EventTraceActivity traceActivityId,
            List<string> exposedFlightFeatures,
            IAccountServiceAccessor accountServiceAccessor,
            IAddressEnrichmentServiceAccessor addressEnrichmentServiceAccessor,
            string scenario,
            string addressType,
            bool usePidlPage,
            bool useV2UXForPidlPage,
            PaymentExperienceSetting setting)
        {
            Address addressEnrichment = new Address(address);

            // Since AVS look up API suggested address line 1 is missing for P.O.BOX. 
            // Per AVS team recommendation we should use validate address API instead.
            AddressLookupResponse lookupResponse;
            try
            {
                lookupResponse = await ValidateAddressWrapper(accountServiceAccessor, addressEnrichmentServiceAccessor, addressEnrichment, traceActivityId, exposedFlightFeatures);
            }
            catch (Exception ex)
            {
                // in case of an address enrichment service exception, return the user entered address  
                SllWebLogger.TracePXServiceException($"SuggestAddressTradeAvsPidl validateAddress Wrapper throw an exception: {ex.ToString()}", traceActivityId);
                lookupResponse = null;
            }

            List<PXAddressV3Info> suggestedAddresses = null;
            List<PXAddressV3Info> addressList = new List<PXAddressV3Info> { address };

            if (lookupResponse?.Status == EnrichmentValidationStatus.Verified ||
                lookupResponse?.Status == EnrichmentValidationStatus.VerifiedShippable)
            {
                // Case 1 : If verified or verifiedshippable
                // If country is US, and user entered address has only 5 digit zipcode, and suggested address is empty, then return consent page.
                // Else if country is US, and user entered address has only 5 digit zipcode, and suggested address has 9 digit zipcode, then return a mergeData client action.
                // Otherwise just return the payload from address enrichement service
                SuggestedAddress suggestedAddress = lookupResponse?.SuggestedAddresses?.FirstOrDefault();
                string suggestedAddressPostalCode = suggestedAddress?.Address?.PostalCode;

                if (string.Equals(address.Country, "us", StringComparison.InvariantCultureIgnoreCase)
                    && suggestedAddress == null
                    && address.PostalCode != null 
                    && address.PostalCode.Length == 5)
                {
                    return GetSuggestedAddressBasedPartner(suggestedAddresses, address, lookupResponse, addressList, exposedFlightFeatures, addressType, usePidlPage, useV2UXForPidlPage, partner, operation, language, scenario, setting);
                }
                else if (string.Equals(address.Country, "us", StringComparison.InvariantCultureIgnoreCase) &&
                    suggestedAddress?.Address?.PostalCode?.Length == 10 &&
                    address.PostalCode.Length == 5)
                {
                    AddressInfoV3 address9digitZip = lookupResponse?.SuggestedAddresses?.FirstOrDefault()?.ToAddressInfoV3(address);
                    if (address9digitZip != null)
                    {
                        // to fix known issue, lookup API set addressline_1 to null for PO BOX
                        if (string.IsNullOrEmpty(address9digitZip.AddressLine1))
                        {
                            address.PostalCode = address9digitZip.PostalCode;
                            address9digitZip = address;
                        }
                    }

                    PXAddressV3Info verifiedAddress = new PXAddressV3Info(address9digitZip);
                    verifiedAddress.IsAVSFullValidationSucceeded = true;
                    MergeDataActionContext mergeDataActionContext = new MergeDataActionContext();
                    mergeDataActionContext.Payload = AddressSelectionHelper.ExtractAddressInfoForTradeAvs(verifiedAddress, addressType);
                    return new List<PIDLResource>()
                        {
                            new PIDLResource()
                            {
                                ClientAction = new ClientAction(ClientActionType.MergeData, mergeDataActionContext)
                            }
                        };
                }
                else
                {
                    MergeDataActionContext mergeDataActionContext = new MergeDataActionContext();
                    PIDLData mergePayload = new PIDLData();
                    mergePayload[GlobalConstants.CommercialZipPlusFourPropertyNames.IsAvsFullValidationSucceeded] = true;
                    mergeDataActionContext.Payload = mergePayload;
                    return new List<PIDLResource>()
                    {
                        new PIDLResource()
                        {
                            ClientAction = new ClientAction(ClientActionType.MergeData, mergeDataActionContext)
                        }
                    };
                }
            }
            else if (lookupResponse?.Status == EnrichmentValidationStatus.InteractionRequired ||
                lookupResponse?.Status == EnrichmentValidationStatus.PremisesPartial ||
                lookupResponse?.Status == EnrichmentValidationStatus.StreetPartial)
            {
                // Case 2: InteractionRequired: Suggested Pages should be shown.
                suggestedAddresses = lookupResponse?.SuggestedAddresses
                    ?.Select((suggestAddress, index) => suggestAddress.ToPXAddressV3Info(index.ToString(), address))
                    .ToList();
            }

            // Case 3: for SuggestAddressesTradeAVS scenario, if lookupResponse.Status is None/NotValidated/Multiple or an unkown status, or an address enrichment service exception, return the user entered addres
            if (suggestedAddresses != null && suggestedAddresses.Count > 0)
            {
                addressList.AddRange(suggestedAddresses);
            }

            // For tradeAVS UX V2, the user entered address is the last option in the list, so move the entered address to the last position in the list.
            if (useV2UXForPidlPage)
            {
                addressList.RemoveAt(0);
                addressList.Add(address);
            }

            return GetSuggestedAddressBasedPartner(suggestedAddresses, address, lookupResponse, addressList, exposedFlightFeatures, addressType, usePidlPage, useV2UXForPidlPage, partner, operation, language, scenario, setting);
        }

        public static async Task<List<PIDLResource>> PostAddress(
            IAccountServiceAccessor accountServiceAccessor,
            string accountId,
            PXAddressV3Info address,
            string apiVersion, 
            int syncToLegacyCode,
            EventTraceActivity traceActivityId, 
            bool setAsDefaultBilling,
            List<string> exposedFlightFeatures)
        {
            var createdAddress = await accountServiceAccessor.PostAddress(accountId, address, apiVersion, syncToLegacyCode, traceActivityId);
            if (setAsDefaultBilling)
            {
                createdAddress.DefaultBillingAddress = true;
                AccountProfileV3 profile = await accountServiceAccessor.GetProfileV3(accountId, GlobalConstants.ProfileTypes.Consumer, traceActivityId);
                profile.DefaultAddressId = createdAddress.Id;
                await accountServiceAccessor.UpdateProfileV3(accountId, profile, GlobalConstants.ProfileTypes.Consumer, traceActivityId, exposedFlightFeatures);
            }

            var pidlDoc = new List<PIDLResource>()
            {
                new PIDLResource()
                {
                        ClientAction = new ClientAction(ClientActionType.ReturnContext, createdAddress)
                }
            };
            return pidlDoc;
        }

        public static async Task<AddressValidateResponse> AddressEnrichmentHelper(
            IAccountServiceAccessor accountServiceAccessor,
            IAddressEnrichmentServiceAccessor addressEnrichmentServiceAccessor,
            Address address,
            EventTraceActivity traceActivityId,
            List<string> exposedFlightFeatures)
        {
            bool useJarvisForAddressEnrichment = exposedFlightFeatures != null && exposedFlightFeatures.Contains(Flighting.Features.PXUseJarvisAccountsForAddressEnrichment, StringComparer.OrdinalIgnoreCase);

            if (useJarvisForAddressEnrichment)
            {
                return await GetJarvisAVSResponse(accountServiceAccessor, address, traceActivityId);
            }
            else if (exposedFlightFeatures != null && exposedFlightFeatures.Contains(Flighting.Features.PXMakeAccountsAddressEnrichmentCall, StringComparer.OrdinalIgnoreCase))
            {
                try
                {
                    var accountsResponseTask = GetJarvisAVSResponse(accountServiceAccessor, address, traceActivityId);
                    var enrichmentResponseTask = addressEnrichmentServiceAccessor.ValidateAddress(address, traceActivityId);
                    var accountsResponse = await accountsResponseTask;
                    var enrichmentResponse = await enrichmentResponseTask;
                    var accountsResponseJson = JsonConvert.SerializeObject(accountsResponse);
                    var enrichmentResponseJson = JsonConvert.SerializeObject(enrichmentResponse);
                    SllWebLogger.TraceServerMessage(
                        "AddressEnrichmentComparison",
                        traceActivityId.ToString(),
                        null,
                        $"AddressEnrichmentHelper: AreEqual: {string.Equals(accountsResponseJson,enrichmentResponseJson, StringComparison.OrdinalIgnoreCase)}, accountsResponse: {SllLogger.Masker.MaskSingle(accountsResponseJson)}, enrichmentResponse: {SllLogger.Masker.MaskSingle(enrichmentResponseJson)}",
                        EventLevel.Informational);
                    return (exposedFlightFeatures != null && exposedFlightFeatures.Contains(Flighting.Features.PXUseJarvisAccountsForAddressEnrichment, StringComparer.OrdinalIgnoreCase)) ? accountsResponse : enrichmentResponse;
                }
                catch (Exception ex)
                {
                    SllWebLogger.TracePXServiceException($"AddressEnrichmentHelper threw an exception: {ex}", traceActivityId);
                }
            }
            
            return await addressEnrichmentServiceAccessor.ValidateAddress(address, traceActivityId);
        }

        private static async Task<AddressValidateResponse> GetJarvisAVSResponse(IAccountServiceAccessor accountServiceAccessor, Address address, EventTraceActivity traceActivityId)
        {
            bool regionIsoEnabled = AddressEnrichmentService.V7.Constants.CountriesRequiredRegionIsoEnabledFlag.Contains(address.Country, StringComparer.InvariantCultureIgnoreCase);
            var validateResponse = await accountServiceAccessor.ModernValidateAddress<AddressValidateResponse>(address, traceActivityId, regionIsoEnabled);

            // AVS can return either a single object in suggestedAddress or multiple in suggestedAddresses
            if (validateResponse.SuggestedAddress != null)
            {
                validateResponse.SuggestedAddresses = new List<Address>
                {
                    validateResponse.SuggestedAddress
                };
                validateResponse.SuggestedAddress = null;
            }

            if (validateResponse.SuggestedAddresses != null)
            {
                TransformRegionCodeAVSToPIDLFormat(address.Country, validateResponse.SuggestedAddresses);
            }

            return validateResponse;
        }

        private static void TransformRegionCodeAVSToPIDLFormat(string countryCode, List<Address> suggestedAddresses)
        {
            // PX has full region codes (e.g. VE-M) for the country Venezuela (ve) whereas AVS returns a single char region code (e.g. M)
            if (string.Equals(CountryCodes.VE, countryCode, StringComparison.InvariantCultureIgnoreCase))
            {
                foreach (Address suggestedAddress in suggestedAddresses)
                {
                    suggestedAddress.Region = suggestedAddress.Region != null ? $"{CountryCodes.VE}-{suggestedAddress.Region}" : suggestedAddress.Region;
                }
            }
            else
            {
                foreach (Address suggestedAddress in suggestedAddresses)
                {
                    if (suggestedAddress.Region != null)
                    {
                        string pidlRegion = AddressEnrichmentServiceConstants.RegionMappingFromPIDLToAVS.FirstOrDefault(kv => string.Equals(kv.Value, suggestedAddress.Region, StringComparison.InvariantCultureIgnoreCase)).Key;
                        suggestedAddress.Region = string.IsNullOrEmpty(pidlRegion) ? suggestedAddress.Region : pidlRegion;
                    }
                }
            }
        }

        private static async Task<AddressLookupResponse> ValidateAddressWrapper(
            IAccountServiceAccessor accountServiceAccessor,
            IAddressEnrichmentServiceAccessor addressEnrichmentServiceAccessor,
            Address addressEnrichment,
            EventTraceActivity traceActivityId,
            List<string> exposedFlightFeatures)
        {
            var validateResponse = await AddressEnrichmentHelper(accountServiceAccessor, addressEnrichmentServiceAccessor, addressEnrichment, traceActivityId, exposedFlightFeatures);

            return new AddressLookupResponse()
            {
                OriginalAddress = validateResponse.OriginalAddress,
                SuggestedAddresses = validateResponse.SuggestedAddresses
                ?.Select(
                    addr =>
                    new SuggestedAddress()
                    {
                        Address = addr
                    })
                .ToList(),
                Status = validateResponse.Status,
                ValidationMessage = validateResponse.ValidationMessage
            };
        }

        private static List<PIDLResource> GetSuggestedAddressBasedPartner(
            List<PXAddressV3Info> suggestedAddresses,
            PXAddressV3Info address,
            AddressLookupResponse lookupResponse,
            List<PXAddressV3Info> addressList,
            List<string> exposedFlightFeatures,
            string addressType,
            bool usePidlPage,
            bool useV2UXForPidlPage,
            string partner,
            string operation,
            string language,
            string scenario,
            PaymentExperienceSetting setting)
        {
            // Get the pidl similar to Add Address flow for XboxNative partners
            if (PXCommon.Constants.PartnerGroups.IsXboxNativePartner(partner) && string.Equals(operation, V7.Constants.Operations.ValidateInstance, StringComparison.OrdinalIgnoreCase))
            {
                if (suggestedAddresses == null)
                {
                    suggestedAddresses = new List<PXAddressV3Info>();
                }

                return AddressSelectionHelper.GetSuggestedAddressesPidls(
                    suggestedAddresses,
                    address,
                    partner,
                    language,
                    operation,
                    lookupResponse?.Status.ToString(),
                    null,
                    addressType: Constants.AddressTypes.PXV3Billing,
                    false,
                    scenario: null);
            }
            else
            {
                return AddressSelectionHelper.GetSuggestedAddressesTradeAvsPidls(
                     addressList,
                     address,
                     partner,
                     language,
                     operation,
                     exposedFlightFeatures,
                     addressType,
                     scenario,
                     usePidlPage,
                     useV2UXForPidlPage,
                     setting);
            }
        }
    }
}