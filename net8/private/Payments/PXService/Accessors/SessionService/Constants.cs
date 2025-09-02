// <copyright file="Constants.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.SessionService.V7
{
    public static class Constants
    {
        internal static class ServiceNames
        {
            internal const string SessionService = "SessionService";
        }

        internal static class VersionNumber
        {
            internal const string V3 = "2015-09-23";
        }

        internal static class Status
        {
            public const string Complete = "COMPLETE";
            public const string Incomplete = "INCOMPLETE";
        }

        internal static class UriTemplate
        {
            internal const string GenerateSessionId = "sessionservice/sessions/GenerateId";
            internal const string GetSessionWithType = "sessionservice/sessions/{0}?sessionType={1}";
            internal const string Session = "sessionservice/sessions/{0}";
        }
    }
}