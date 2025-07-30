// <copyright file="AzureExPAccessor.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>
using Azure.Analytics.Experimentation;

namespace Microsoft.Commerce.Payments.PXService
{
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Commerce.Payments.Authentication;
    using Microsoft.Commerce.Payments.Authentication.AzureActiveDirectoryCaller;
    using Microsoft.Commerce.Payments.Common;
    using Microsoft.Commerce.Payments.Common.Tracing;
    using Microsoft.Commerce.Payments.Common.Web;
    using Microsoft.Commerce.Payments.PXCommon;
    using Microsoft.Extensions.Logging;
    using static Microsoft.Commerce.Payments.PXCommon.Flighting;

    public class AzureExPAccessor : IAzureExPAccessor
    {
        private VariantAssignmentHttpPollingProvider variantAssignmentProvider = null;
        private bool enableTestHook = false;
        private int vaInitCheckCounter = 0;

        public class AzureExPLogger : ILogger, IDisposable
        {
            public IDisposable BeginScope<TState>(TState state) => this;

            public bool IsEnabled(LogLevel logLevel)
            {
                return true;
            }

            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
            {
                try
                {
                    if (logLevel == LogLevel.Error || exception != null)
                    {
                        SllWebLogger.TracePXServiceException($"AzureExPFlighting Exception: {exception?.ToString()}. Details: {formatter(state, exception)}", EventTraceActivity.Empty);
                    }
                }
                catch(Exception ex)
                {
                    SllWebLogger.TracePXServiceException($"AzureExPFlighting Exception: {ex?.ToString()}", EventTraceActivity.Empty);
                }
            }

            public void Dispose()
            {
                GC.SuppressFinalize(this);
            }
        }

        public AzureExPAccessor(
            string expBlobUrl,
            IAzureActiveDirectoryTokenLoader tokenLoader,
            HttpMessageHandler messageHandler,
            bool enableTestHook = false)
        {
            try
            {
                this.enableTestHook = enableTestHook;
                if (!string.IsNullOrWhiteSpace(expBlobUrl))
                {
                    //// Stylecop is throwing an error with code 'SA0102' on this line and we couldn't suppress it. So, disabled the stylecop for the entire file.
                    AuthorizationHeaderHandler authenticationHandler = new AuthorizationHeaderHandler((r, c) => tokenLoader.GetTokenStringAsync(null, default).ContinueWith<(string, string)>(t => ("Bearer", t.Result)))
                    { InnerHandler = messageHandler };


                    var azureExPPollingClient = new PXTracingHttpClient(PXCommon.Constants.ServiceNames.AzureExPService, authenticationHandler);
                    azureExPPollingClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(PaymentConstants.HttpMimeTypes.JsonContentType));
                    azureExPPollingClient.DefaultRequestHeaders.Add(PaymentConstants.HttpHeaders.Connection, PaymentConstants.HttpHeaders.KeepAlive);
                    azureExPPollingClient.DefaultRequestHeaders.Add(PaymentConstants.HttpHeaders.KeepAlive, string.Format(PaymentConstants.HttpHeaders.KeepAliveParameter, 60));

                    var pollingInterval = TimeSpan.FromSeconds(60);
                    var pollingUrl = new Uri(expBlobUrl);
                    ILogger logger = new AzureExPLogger();

                    //Initialize Variant Assignment provider and set not to throw error if not initialized. Make sure to not start the provider here, as it may cause longer for app initialization time.
                    var httpClient = new HttpClient(authenticationHandler);
                    this.variantAssignmentProvider = new VariantAssignmentHttpPollingProvider(azureExPPollingClient, pollingUrl, pollingInterval, logger) { ThrowUntilInitialized = false };
                }
                else
                {
                    SllWebLogger.TracePXServiceException("AzureExPBlob Url is not specified", EventTraceActivity.Empty);
                }
            }
            catch (Exception ex)
            {
                SllWebLogger.TracePXServiceException("Flighting.Initialization: " + ex.ToString(), EventTraceActivity.Empty);
            }
        }

        public async Task<FeatureConfig> GetExposableFeatures(Dictionary<string, string> flightContext, EventTraceActivity traceActivityId)
        {
            FeatureConfig featureConfig = null;
            try
            {
                if (variantAssignmentProvider != null)
                {
                    // Check for InitializationTask only once and wait for only 2 seconds for the first request. Let's not check for every request.
                    if (this.vaInitCheckCounter == 0)
                    {
                        if (Interlocked.Exchange(ref this.vaInitCheckCounter, 1) == 0)
                        {
                            SllWebLogger.TraceServerMessage(
                                "AzureExPVAProvider",
                                traceActivityId.ToString(),
                                null,
                                "Starting VAProvider",
                                Diagnostics.Tracing.EventLevel.Warning);

                            // start VAProvider
                            this.variantAssignmentProvider.Start();
                            var vaInitTask = this.variantAssignmentProvider.InitializationTask;
                            if (await Task.WhenAny(vaInitTask, Task.Delay(TimeSpan.FromSeconds(2))) != vaInitTask)
                            {
                                SllWebLogger.TracePXServiceException("AzureExPAccessor: couldn't complete variantAssingmentProvider InitializationTask within the specified timeout period", traceActivityId);
                            }
                        }
                    }

                    // set request parameters with context data
                    var vaRequest = PrepareVariantAssignmentRequest(flightContext);

                    // get variant assignments. Timeout after 2 secs.
                    var vaAssignmentTask = this.variantAssignmentProvider.GetVariantAssignmentsAsync(vaRequest);
                    if (await Task.WhenAny(vaAssignmentTask, Task.Delay(TimeSpan.FromSeconds(2))) == vaAssignmentTask)
                    {
                        using (var variantAssignmentResponse = await vaAssignmentTask)
                        {
                            // extact variant assingment response
                            if (variantAssignmentResponse != null)
                            {
                                featureConfig = ExtractFeatureConfig(variantAssignmentResponse);
                            }
                            else
                            {
                                SllWebLogger.TracePXServiceException("AzureExPAccessor: variantAssignmentResponse object is null", traceActivityId);
                            }
                        }
                    }
                    else
                    {
                        SllWebLogger.TracePXServiceException("AzureExPAccessor: couldn't get variantAssignmentResponse within the specified timeout period", traceActivityId);
                    }
                }
                else
                {
                    SllWebLogger.TracePXServiceException("AzureExPAccessor: variantAssignmentProvider object is null", traceActivityId);
                }
            }
            catch (Exception ex)
            {
                SllWebLogger.TracePXServiceException("AzureExPAccessor: " + ex.ToString(), traceActivityId);
            }

            return featureConfig ?? new FeatureConfig();
        }

        public void StopPolling()
        {
            this.variantAssignmentProvider?.Stop();
        }

        public bool InitializeVariantAssignmentProvider(byte[] blobContent)
        {
            // support variant assignment provider initialization from a given blob content, only when test hook is enabled
            if (this.enableTestHook)
            {
                return this.variantAssignmentProvider.TryInitializeVariantAssignmentProvider(blobContent);
            }

            return false;
        }

        private static VariantAssignmentRequest PrepareVariantAssignmentRequest(Dictionary<string, string> flightContext)
        {
            var vaRequest = new VariantAssignmentRequest();

            // set request parameters with context data
            if (flightContext != null)
            {
                foreach (var ctx in flightContext)
                {
                    if (!string.IsNullOrEmpty(ctx.Value))
                    {
                        vaRequest.Parameters.Add(ctx.Key, ctx.Value);
                    }
                }
            }

            return vaRequest;
        }

        private static FeatureConfig ExtractFeatureConfig(IVariantAssignmentResponse variantAssignmentResponse)
        {
            List<string> enabledFeatures = new List<string>();
            string assignmentContext = string.Empty;
            const string DEFAULT_NAMESPACE = "default.";

            if (variantAssignmentResponse != null)
            {
                var featureVariablesRes = variantAssignmentResponse.GetFeatureVariables();
                assignmentContext = variantAssignmentResponse.GetAssignmentContext();

                // prepare features list
                foreach (var feature in featureVariablesRes)
                {
                    string featureName = string.Join(".", feature.KeySegments);

                    // Remove default namespace prefix to match with our existing feature names.
                    if (featureName.StartsWith(DEFAULT_NAMESPACE, StringComparison.OrdinalIgnoreCase))
                    {
                        featureName = featureName.Substring(DEFAULT_NAMESPACE.Length);
                    }

                    switch (feature.ValueKind)
                    {
                        case FeatureVariableValueKind.Boolean:
                            if (feature.GetBooleanValue())
                            {
                                enabledFeatures.Add(featureName);
                            }
                            break;
                        default:
                            break;
                    }
                }
            }

            return new FeatureConfig(assignmentContext, enabledFeatures);
        }
    }
}