// <copyright file="PartnerSettingsHelper.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Threading.Tasks;
    using Microsoft.Commerce.Payments.Common.Tracing;
    using Microsoft.Commerce.Payments.Common.Web;
    using Microsoft.Commerce.Payments.PartnerSettingsModel;
    using Microsoft.Commerce.Payments.PidlFactory;
    using Microsoft.Commerce.Payments.PidlFactory.V7;
    using Microsoft.Commerce.Payments.PXCommon;
    using Microsoft.Commerce.Payments.PXService.Settings;

    public class PartnerSettingsHelper
    {
        // Tenant ID and partner name mapper
        public static IReadOnlyDictionary<string, string> TenantIdPartnerNameMapper
        {
            get
            {
                return new Dictionary<string, string>()
                {
                    { "tn_6298182379cd44bfaac3ae3e96505175", V7.Constants.PartnerName.CandyCrush }, // AlphaStore Sandbox
                    { "tn_f66434e79e38412cab7b821edae50515", V7.Constants.PartnerName.CandyCrush }, // AlphaStore Prod
                    { "tn_3f1b8c1e2c3e4c5f8b6e7d8e9f0a1b2c", V7.Constants.PartnerName.BattleNet }, // BattleNet Sandbox
                    { "tn_7a8d7a05ac2245a894c9399f54bb4f35", V7.Constants.PartnerName.BattleNet }, // BattleNet Prod
                };
            }
        }

        public static async Task<PartnerSettings> GetPaymentExperienceSetting(PXServiceSettings pxSettings, string partner, string partnerSettingsVersion, EventTraceActivity traceActivityId, List<string> exposableFeatures)
        {
            // Use Partner Setttings Service if flight is enabled or if parnter PIDL configuration does not exist
            try
            {
                partner = (string.IsNullOrWhiteSpace(partner) ? Constants.PartnerNames.DefaultPartnerName : partner).ToLower();
                if (PartnerShouldUsePartnerSettingsService(partner, exposableFeatures))
                {
                    partnerSettingsVersion = partnerSettingsVersion ?? GetPartnerSettingsVersion(exposableFeatures);
                    return new PartnerSettings { PaymentExperienceSettings = await pxSettings.PartnerSettingsServiceAccessor.GetPaymentExperienceSettings(partner, partnerSettingsVersion, traceActivityId, exposableFeatures) };
                }
            }
            catch (Exception ex)
            {
                SllWebLogger.TracePXServiceException($"Exception Calling Partner Settings Service: " + ex.ToString(), traceActivityId);

                // Throw PX Exception if PSS gives 400/404/405 and flight is enabled
                if (exposableFeatures.Contains(Flighting.Features.PXEnforcePSSPartnerValidation) && ex is ServiceErrorResponseException)
                {
                    var serviceException = ex as ServiceErrorResponseException;
                    if (string.Equals(serviceException.Error?.ErrorCode, HttpStatusCode.NotFound.ToString()) || string.Equals(serviceException.Error?.ErrorCode, HttpStatusCode.BadRequest.ToString()) || string.Equals(serviceException.Error?.ErrorCode, HttpStatusCode.MethodNotAllowed.ToString()))
                    {
                        throw new PIDLArgumentException(ErrorConstants.ErrorMessages.InvalidPartner, ErrorConstants.ErrorCodes.InvalidPartner);
                    }
                }
            }

            return null;
        }

        private static bool PartnerShouldUsePartnerSettingsService(string partner, List<string> exposableFeatures)
        {
            if (string.Equals(partner, Constants.PartnerNames.DefaultPartnerName, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            if (TemplateHelper.IsTemplateBasedPIDLIncludingDefaultTemplate(partner) || Constants.PartnerGroups.IsTestPartner(partner) || Constants.PartnerGroups.IsNoConfigPartner(partner))
            {
                return false;
            }

            return exposableFeatures.Contains(Flighting.Features.PXUsePartnerSettingsService) || !PIDLResourceFactory.CheckPartnerPIDLConfigurationExists(partner);
        }

        private static string GetPartnerSettingsVersion(List<string> exposableFeatures)
        {
            if (exposableFeatures != null)
            {
                string partnerSettingsVersionPartnerFlight = exposableFeatures.Where(flightValue => flightValue.StartsWith(V7.Constants.PartnerFlightValues.PartnerSettingsVersionPrefix, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
                if (!string.IsNullOrEmpty(partnerSettingsVersionPartnerFlight) && partnerSettingsVersionPartnerFlight.Length > V7.Constants.PartnerFlightValues.PartnerSettingsVersionPrefix.Length)
                {
                    return partnerSettingsVersionPartnerFlight.Substring(V7.Constants.PartnerFlightValues.PartnerSettingsVersionPrefix.Length).ToLowerInvariant();
                }
            }

            return null;
        }
    }
}