// <copyright file="PXServiceAircapiSettings.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Settings
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using Microsoft.Commerce.Payments.PXService.Model.Authentication;
    using Microsoft.IdentityModel.S2S.Configuration;
    using Newtonsoft.Json;

    [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling", Justification = "Don't need the excessive class coupling check here")]
    public class PXServiceAircapiSettings : PXServiceSettings
    {
        [SuppressMessage("Microsoft.Performance", "CA1801", Justification = "EnvironmentName is needed to adhere to the convention.")]
        [SuppressMessage("Microsoft.Maintainability", "CA1506: Avoid excessive class coupling", Justification = "Central class initialization method.")]
        public PXServiceAircapiSettings()
        {
            this.AzureExpEnabled = false;

            this.LocalFeatureConfigs = PXServiceSettings.FetchStaticFeatureConfigs(
                "Settings\\FeatureConfig\\featureconfigs.json",
                "Settings\\FeatureConfig\\testusergroups.json",
                "int_test_user_group");

            ResourceLifecycleStateManager.Initialize(
                this,
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ResourceLifecycleStateManager.ResourceLifecycleConstants.ErrorConfigFilePath),
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ResourceLifecycleStateManager.ResourceLifecycleConstants.ClientActionConfigFilePath));

            this.JsonSerializerSettings = new JsonSerializerSettings();
            this.JsonSerializerSettings.NullValueHandling = NullValueHandling.Ignore;
            this.JsonSerializerSettings.Converters.Add(new Newtonsoft.Json.Converters.StringEnumConverter());

            this.PifdBaseUrl = "https://pifd.cp.microsoft-int.com/V6.0";

            this.PayMicrosoftBaseUrl = "https://payint.microsoft.com";

            this.StaticResourceServiceBaseUrl = "https://staticresources.payments.microsoft-int.com/staticresourceservice";

            this.InitializeAuthenticationSettings();
        }

        // consider refactor the settings
        private void InitializeAuthenticationSettings()
        {
            IList<UserInformation> partnerInformationList = new List<UserInformation>()
            {
                new UserInformation()
                {
                    Role = GlobalConstants.ClientRoles.Test,
                    AllowedAccounts = "18c69db8-dd2e-48a0-8887-1ccabd0bbcb2,3cfa0e51-97ae-49a8-9a71-398ca2ba0683,62dc8681-6753-484a-981a-128f82a43d25,7e5242d0-33ea-4bd1-a691-5193af93c4c7,ec8c235c-65e2-4a3d-bd7d-a20ed8ec1688",
                    AllowedAuthenticatedPathTemplate = "/v7.0/{0}",
                    AllowedUnAuthenticatedPaths = "/v7.0/settings/Microsoft.Payments.Client,/v7.0/addresses/legacyValidate,/v7.0/addresses/modernValidate,/v7.0/paymentSessions,/v7.0/sessions,/v7.0/checkoutsEx,/v7.0/checkoutDescriptions",
                    PartnerName = Partner.Name.PXCOT.ToString(),
                    ApplicationId = "7033f9b1-b4e6-4d49-9bde-738d53c14ae9" // Application name: PX-COT-INT-PME 
                },
                new UserInformation()
                {
                    Role = GlobalConstants.ClientRoles.Admin,
                    PartnerName = Partner.Name.PIFDService.ToString(),
                    CertificateVerificationRule = new Management.CertificateVerificationCore.VerifyBySubjectIssuerThumbprint(
                        "CN=clientauth-pifd.pims-int.azclient.ms",
                        new List<Management.CertificateVerificationCore.IssuerGroup>()
                        {
                            Management.CertificateVerificationCore.IssuerGroup.AME
                        }),
                    ApplicationId = "1cac7046-9cb1-40ab-9f13-efd1f778b3cc" // Application name: PaymentInstrumentFrontDoor, tenant id (First Party PPE) : ea8a4392-515e-481f-879e-6571ff2a8a36
                }
            };

            var uberUserDirectory = new UberUserDirectory(partnerInformationList);

            var aadAuthOptions = new AadAuthenticationOptions
            {
                ClientId = "ede6832e-2581-4c10-8141-9b4cbe81e06c",  // Payment Experience Service INT FPA Client ID
                TenantId = "975f013f-7f24-47e8-a7d3-abc4752bf346",  // PX PME Tenant ID
                Audience = "https://paymentexperience-test.cp.microsoft-int.com/",
                Instance = "https://sts.windows.net/"
            };

            this.AuthorizationFilter = new PXServiceAuthorizationFilterAttribute
            {
                // TODO: need to set AllowUnauthenticatedHttpsCalls to be false when PIFD is in AirCapi.
                AllowUnauthenticatedHttpsCalls = true,
                AllowUnauthenticatedHttpCalls = true,
                UberUserDirectory = uberUserDirectory
            };

            this.AuthorizationFilter.TokenMiseValidator = new PXCommon.TokenMiseValidator(aadAuthOptions, new AuthenticationLogger());

            this.AuthorizationFilter.CertificateAuthenticator = new Management.CertificateVerificationCore.UserDirectory(
                users: uberUserDirectory.CertificateVerificationRules,
                online: true,
                issuerFetcher: null,
                verifyRootCA: false,
                verifyOfflineRevocation: false,
                verifyExpirationTime: true,
                logger: new CertificateLogger());
        }
    }
}