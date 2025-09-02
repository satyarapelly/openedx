// <copyright file="AuditLogger.cs" company="Microsoft">Copyright (c) Microsoft 2016. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.Common.Web
{
    using System;
    using System.Net.Http;
    using Microsoft.Commerce.Payments.Common.Tracing;
    using Microsoft.Commerce.Payments.Common.Web;
    using Microsoft.Extensions.Logging;
    using global::OpenTelemetry.Audit.Geneva;
    using System.Diagnostics.Tracing;

    public static class AuditLogger
    {
        private const string OperationAccessLevel = "Default";
        private const string PXResourceType = "API";
        private static AuditLoggerFactory auditFactory;
        private static ILogger dataPlaneLogger;

        public static void Instantiate()
        {
            // Documentation: https://eng.ms/docs/products/geneva/collect/instrument/opentelemetryaudit/dotnet/windows/installation
            // For WebApp/WebJob/AzureFunction/AppService/Autopilot use AuditOptions.DefaultForEtw
            //  For other platforms use AuditOptions.DefaultForSecurityEvent
            auditFactory = AuditLoggerFactory.Create(AuditOptions.DefaultForEtw);
            dataPlaneLogger = auditFactory.CreateDataPlaneLogger(); // Target table to find the logs: AsmAuditDP Jarvis table
        }

        public static void AuditIncomingCall(
            string operationName,
            HttpRequestMessage request,
            HttpResponseMessage response)
        {
            // Documentation: https://1dsdocs.azurewebsites.net/schema/PartB/logs/Audit.html
            try
            {
                if (LoggingConfig.Mode == LoggingMode.Sll)
                {
                    SllWebLogger.TraceServerMessage("IncomingRequestAuditLogging", request.GetRequestCorrelationId().ActivityId.ToString(), null, "Started audit logging", EventLevel.Informational);
                }
                else if (LoggingConfig.Mode == LoggingMode.OpenTelemetry)
                {
                    Logger.Qos.TraceServerMessage("IncomingRequestAuditLogging", request.GetRequestCorrelationId().ActivityId.ToString(), null, "Started audit logging", QosEventLevel.Information);
                }
                else
                {
                    SllWebLogger.TraceServerMessage("IncomingRequestAuditLogging", request.GetRequestCorrelationId().ActivityId.ToString(), null, "Started audit logging", EventLevel.Informational);
                    Logger.Qos.TraceServerMessage("IncomingRequestAuditLogging", request.GetRequestCorrelationId().ActivityId.ToString(), null, "Started audit logging", QosEventLevel.Information);
                }

                object callerObject;
                request.Options.TryGetValue(new HttpRequestOptionsKey<object>(PaymentConstants.Web.Properties.CallerName), out callerObject);
                var auditRecord = new AuditRecord
                {
                    OperationName = operationName,
                    OperationResult = response != null && response.IsSuccessStatusCode ? OperationResult.Success : OperationResult.Failure,
                    OperationResultDescription = response.StatusCode.ToString() ?? string.Empty,
                    CallerIpAddress = request.GetClientIP(),
                    OperationAccessLevel = OperationAccessLevel,
                    OperationType = GetOperationType(request.Method.Method),
                    CallerAgent = request.GetUserAgent()
                };

                auditRecord.AddOperationCategory(OperationCategory.ResourceManagement);

                // if caller identity is "Other", then description in caller Entry is mandatory 
                auditRecord.AddCallerIdentity(global::OpenTelemetry.Audit.Geneva.CallerIdentityType.Other, (string)callerObject ?? string.Empty, "Sample Description");
                auditRecord.AddTargetResource(PXResourceType, request.RequestUri?.AbsoluteUri ?? string.Empty);
                auditRecord.AddCallerAccessLevel(OperationAccessLevel);
                dataPlaneLogger.LogAudit(auditRecord);
                if (LoggingConfig.Mode == LoggingMode.Sll)
                {
                    SllWebLogger.TraceServerMessage("IncomingRequestAuditLogging", request.GetRequestCorrelationId().ActivityId.ToString(), null, "AuditLogs-Collected AuditRecord successfully", EventLevel.Informational);
                }
                else if (LoggingConfig.Mode == LoggingMode.OpenTelemetry)
                {
                    Logger.Qos.TraceServerMessage("IncomingRequestAuditLogging", request.GetRequestCorrelationId().ActivityId.ToString(), null, "AuditLogs-Collected AuditRecord successfully", QosEventLevel.Information);
                }
                else
                {
                    SllWebLogger.TraceServerMessage("IncomingRequestAuditLogging", request.GetRequestCorrelationId().ActivityId.ToString(), null, "AuditLogs-Collected AuditRecord successfully", EventLevel.Informational);
                    Logger.Qos.TraceServerMessage("IncomingRequestAuditLogging", request.GetRequestCorrelationId().ActivityId.ToString(), null, "AuditLogs-Collected AuditRecord successfully", QosEventLevel.Information);
                }
            }
            catch (Exception ex)
            {
                if (LoggingConfig.Mode == LoggingMode.Sll)
                {
                    SllWebLogger.TracePXServiceException("Audit logging exception: " + ex.Message, request.GetRequestCorrelationId());
                }
                else if (LoggingConfig.Mode == LoggingMode.OpenTelemetry)
                {
                    Logger.Qos.TracePXServiceException("Audit logging exception: " + ex.Message, request.GetRequestCorrelationId());
                }
                else
                {
                    SllWebLogger.TracePXServiceException("Audit logging exception: " + ex.Message, request.GetRequestCorrelationId());
                    Logger.Qos.TracePXServiceException("Audit logging exception: " + ex.Message, request.GetRequestCorrelationId());
                }
            }
        }

        // PX service Kusto logs show only GET, POST and PATCH calls for past 30days
        private static OperationType GetOperationType(string requestMethod)
        {
            OperationType operationType;
            switch (requestMethod)
            {
                case "GET":
                    operationType = OperationType.Read;
                    break;
                case "POST":
                    operationType = OperationType.Create;
                    break;
                case "PUT":
                case "PATCH":
                    operationType = OperationType.Update;
                    break;
                case "DELETE":
                    operationType = OperationType.Delete;
                    break;
                default:
                    throw new ValidationException(ErrorCode.InvalidRequestData, "Unsupported Request method " + requestMethod);
            }

            return operationType;
        }
    }
}