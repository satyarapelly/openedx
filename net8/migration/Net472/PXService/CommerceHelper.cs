// <copyright file="CommerceHelper.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService
{
    using Microsoft.Commerce.Payments.Common;
    using Microsoft.Commerce.Payments.Common.Tracing;
    using Microsoft.Commerce.Payments.Common.Web;
    using Microsoft.Commerce.Tracing;

    public class CommerceHelper
    {
        public static void GetIdentity(string altSecId, string orgPuid, EventTraceActivity traceActivityId, out string identityValue, out string identityType)
        {
            if (!string.IsNullOrEmpty(altSecId))
            {
                identityValue = altSecId;
                identityType = "PUID";
            }
            else if (!string.IsNullOrEmpty(orgPuid))
            {
                identityValue = orgPuid;
                identityType = "OrgPUID";
            }
            else            
            {
                throw TraceCore.TraceException(traceActivityId, new ValidationException(ErrorCode.InvalidRequestData, string.Format("response status code: {0}, error: {1}", "InvalidRequestData", "Puid or OrgPuid are required")));
            }
        }
    }
}