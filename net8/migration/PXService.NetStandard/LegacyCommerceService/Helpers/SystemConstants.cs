// <copyright file="SystemConstants.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Accessors.LegacyCommerceService.Helpers
{
    public static class SystemConstants
    {
        public const string DAProfilerTraceTag = "Ctp Web DataAccessor";
        public readonly static int CommerceServiceMaxTryCount = 2;
        public readonly static int CommerceServiceRetryInterval = 200;
        public readonly static int ScsServiceRetryCount = 2;
        public readonly static int DmpServiceRetryCount = 2;
        public readonly static int SapiServiceRetryCount = 2;
        public readonly static int DrsServiceRetryCount = 2;
        public readonly static int RiskManagementServiceRetryCount = 2;
        public readonly static int ChallengeServiceRetryCount = 2;
        public readonly static int BecServiceRetryCount = 2;
        public readonly static int RestApiServiceRetryCount = 2;
    }
}
