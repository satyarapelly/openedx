// <copyright file="AuditLogger.cs" company="Microsoft">
// Copyright (c) Microsoft 2025. All rights reserved.
// </copyright>

using System;
using System.Diagnostics.Tracing;
using Microsoft.AspNetCore.Http;
using Microsoft.Commerce.Payments.Common.OpenTelemetry;
using Microsoft.Commerce.Payments.Common.Tracing;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Audit.Geneva;

namespace Microsoft.Commerce.Payments.Common.Web
{
    public static class AuditLogger
    {
        private const string OperationAccessLevel = "Default";
        private const string PXResourceType = "API";

        private static AuditLoggerFactory auditFactory;
        private static ILogger dataPlaneLogger;

        public static void Instantiate()
        {
            // Geneva ETW setup (for App Services or Functions)
            auditFactory = AuditLoggerFactory.Create(AuditOptions.DefaultForEtw);
            dataPlaneLogger = auditFactory.CreateDataPlaneLogger();
        }

        public static void LogAudit(HttpRequest request, HttpResponse response, string operationName)
        {
            try
            {
                SllWebLogger.TraceServerMessage(
                    "IncomingRequestAuditLogging",
                    request.GetRequestCorrelationId().ActivityId.ToString(),
                    null,
                    "Started audit logging",
                    QosEventLevel.Information);

                request.HttpContext.Items.TryGetValue(PaymentConstants.Web.Properties.CallerName, out var callerObject);

                var auditRecord = new AuditRecord
                {
                    OperationName = operationName,
                    OperationResult = response != null && response.StatusCode < 400 ? OperationResult.Success : OperationResult.Failure,
                    OperationResultDescription = response?.StatusCode.ToString() ?? string.Empty,
                    CallerIpAddress = request.GetClientIP(),
                    OperationAccessLevel = OperationAccessLevel,
                    OperationType = GetOperationType(request.Method),
                    CallerAgent = request.GetUserAgent()
                };

                auditRecord.AddOperationCategory(OperationCategory.ResourceManagement);
                auditRecord.AddCallerIdentity(CallerIdentityType.Other, callerObject?.ToString() ?? string.Empty, "Sample Description");
                auditRecord.AddTargetResource(PXResourceType, request.Path);
                auditRecord.AddCallerAccessLevel(OperationAccessLevel);

                dataPlaneLogger.LogAudit(auditRecord);

                SllWebLogger.TraceServerMessage(
                    "IncomingRequestAuditLogging",
                    request.GetRequestCorrelationId().ActivityId.ToString(),
                    null,
                    "AuditLogs-Collected AuditRecord successfully",
                    QosEventLevel.Information);
            }
            catch (Exception ex)
            {
                SllWebLogger.TracePXServiceException(
                    "Audit logging exception: " + ex.Message,
                    request.GetRequestCorrelationId());
            }
        }

        // PX service Kusto logs show only GET, POST and PATCH calls for past 30 days
        private static OperationType GetOperationType(string requestMethod)
        {
            return requestMethod.ToUpperInvariant() switch
            {
                "GET" => OperationType.Read,
                "POST" => OperationType.Create,
                "PUT" => OperationType.Update,
                "PATCH" => OperationType.Update,
                "DELETE" => OperationType.Delete,
                _ => throw new ArgumentException("Unsupported Request method " + requestMethod, nameof(requestMethod))
            };
        }
    }
}
