// <copyright file="TestConstants.cs" company="Microsoft">Copyright (c) Microsoft 2014. All rights reserved.</copyright>

namespace COT.PXService
{
    using System.Collections.Generic;

    internal static class TestConstants
    {
        private static List<string> listPIPartners = new List<string>()
        {
            TestConstants.PartnerNames.Cart,
            TestConstants.PartnerNames.Webblends,
            TestConstants.PartnerNames.OxoWebDirect
        };

        internal static List<string> ListPIPartners
        {
            get
            {
                return listPIPartners;
            }
        }

        internal static class PartnerNames
        {
            public const string Cart = "cart";
            public const string CommercialStores = "commercialstores";
            public const string Webblends = "webblends";
            public const string OxoWebDirect = "oxowebdirect";
            public const string Xbox = "xbox";
            public const string Bing = "bing";
            public const string WebblendsInline = "webblends_inline";
            public const string Webpay = "webpay";
            public const string Amcweb = "amcweb";
            public const string SetupOffice = "setupoffice";
            public const string SetupOfficeSdx = "setupofficesdx";
            public const string Office = "office";
            public const string Payin = "payin";
            public const string Azure = "azure";
            public const string Storify = "storify";
            public const string Defaulttemplate = "defaulttemplate";
        }

        internal static class PidlBaseUrl
        {
            public const string XForwardedHostValue = "DummyPidlBaseUrl";
            public const string ExpectedUriTemplate = "https://{0}/V6.0";
        }

        internal static class AuthenticationTestConstants
        {
            public const int NumAuthenticationRequests = 3;
        }
    }
}
