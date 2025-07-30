// <copyright file="CertificateLogger.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Model.Authentication
{
    using System;
    using System.Diagnostics;
    using Microsoft.Commerce.Payments.Common.Web;
    using Microsoft.Commerce.Payments.Management.CertificateVerificationCore;

    public class CertificateLogger : ILogger
    {
        void ILogger.LogDebug(string message)
        {
            SllWebLogger.TracePXServiceIntegrationError("getIssuerAPI", IntegrationErrorCode.GetIssuerAPIDebug, string.Format("DebugInfo in getIssuerAPI, message: {0}", message), Guid.NewGuid().ToString());
        }

        void ILogger.LogError(string message, Exception exception, int eventId)
        {
            SllWebLogger.TracePXServiceIntegrationError("getIssuerAPI", IntegrationErrorCode.GetIssuerAPIError, string.Format("Error in getIssuerAPI, message: {0}, exception: {1}", message, exception.ToString()), Guid.NewGuid().ToString());
        }

        void ILogger.LogInformation(string message, int eventId)
        {
            SllWebLogger.TracePXServiceIntegrationError("getIssuerAPI", IntegrationErrorCode.GetIssuerAPIInfo, string.Format("Information in getIssuerAPI, message: {0}", message), Guid.NewGuid().ToString());
        }

        void ILogger.LogWarning(string message, Exception exception, int eventId)
        {
            SllWebLogger.TracePXServiceIntegrationError("getIssuerAPI", IntegrationErrorCode.GetIssuerAPIWarning, string.Format("Warning in getIssuerAPI, message: {0}, exception: {1}", message, exception.ToString()), Guid.NewGuid().ToString());
        }
    }
}