// <copyright file="CommerceServiceExecutor.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Accessors.LegacyCommerceService
{
    using System;
    using System.Security.Cryptography.X509Certificates;
    using System.ServiceModel;
    using System.Text;
    using System.Threading;
    using Microsoft.CommonSchema.Services.Logging;
    using Microsoft.Commerce.Payments.Common.Web;
    using Microsoft.Commerce.Payments.PXService.Accessors.LegacyCommerceService.Helpers;
    using Microsoft.Commerce.Payments.PXService.Accessors.LegacyCommerceService.DataModel;
    using Microsoft.Commerce.Payments.PXService.Accessors.LegacyCommerceService.Messages;
    using Microsoft.Commerce.Payments.Common.Tracing;

    public abstract class CommerceServiceExecutor<TServieChannel>
        where TServieChannel : IDisposable, System.ServiceModel.IClientChannel
    {

        private ITracer logger;
        protected string remoteUrl;

        protected BasicHttpBinding binding;
        protected EndpointAddress address;
        protected X509Certificate2 clientCert;

        protected string CertInformation
        {
            get
            {
                StringBuilder builder = new StringBuilder();
                if (this.clientCert != null)
                {
                    builder.AppendFormat("CertSubject:={0}; ", this.clientCert.Subject);
                    builder.AppendFormat("CertSerialNumber:={0};", this.clientCert.SerialNumber);
                }

                return builder.ToString();
            }
        }

        protected void Initialize()
        {
            this.Logger.Verbose("Initialize Data Accessor : RemoteUrl = {0}", remoteUrl);

            #region Build Binding Object
            this.binding = new BasicHttpBinding(BasicHttpSecurityMode.Transport);
            // Setup basic binding properties
            this.binding.CloseTimeout = TimeSpan.FromMinutes(3);
            this.binding.OpenTimeout = TimeSpan.FromMinutes(3);
            this.binding.ReceiveTimeout = TimeSpan.FromMinutes(5);
            this.binding.SendTimeout = TimeSpan.FromMinutes(5);
            this.binding.BypassProxyOnLocal = false;
            // HostNameComparisonMode property is not available in newer WCF stacks
            this.binding.MaxBufferPoolSize = 524288;
            this.binding.MessageEncoding = WSMessageEncoding.Text;
            this.binding.TextEncoding = Encoding.UTF8;
            this.binding.UseDefaultWebProxy = true;
            this.binding.AllowCookies = false;

            // Setup ReaderQuotas
            binding.MaxReceivedMessageSize = int.MaxValue;
            binding.ReaderQuotas.MaxDepth = int.MaxValue;
            binding.ReaderQuotas.MaxStringContentLength = int.MaxValue;
            binding.ReaderQuotas.MaxArrayLength = int.MaxValue;
            binding.ReaderQuotas.MaxBytesPerRead = int.MaxValue;
            binding.ReaderQuotas.MaxNameTableCharCount = int.MaxValue;

            // Setup Transport
            this.binding.Security.Transport.ClientCredentialType = HttpClientCredentialType.Certificate;
            this.binding.Security.Transport.ProxyCredentialType = HttpProxyCredentialType.None;
            // Realm property is not supported on HttpTransportSecurity in .NET Core/8
            #endregion

            #region Build Endpoint Ojbect
            // Setup Endpoint
            this.address = new EndpointAddress(this.remoteUrl);
            #endregion
        }

        public ITracer Logger
        {
            get
            {
                if (this.logger == null)
                {
                    this.logger = new LogTracer();
                }

                return this.logger;
            }
            set
            {
                this.logger = value;
            }
        }

        public CommerceServiceExecutor(string url, X509Certificate2 clientCert)
        {
            this.remoteUrl = url;
            this.clientCert = clientCert;
            this.Initialize();
        }

        protected abstract string ContractName { get; }

        protected TDataAccessResponse Execute<TDataAccessRequest, TDataAccessResponse, TServiceInput, TServiceOutput>(
            DataAccessorType type,
            TDataAccessRequest request,
            Func<TDataAccessRequest, TServiceInput> constructServiceInput,
            Func<TServieChannel, TServiceInput, TServiceOutput> serviceExecute,
            Func<TServiceOutput, TDataAccessResponse> constructDataAccessOutput,
            string serviceName,
            EventTraceActivity traceActivityId)
                where TDataAccessRequest : AbstractRequest, new()
                where TDataAccessResponse : AbstractResponse, new()
                where TServiceInput : System.Runtime.Serialization.IExtensibleDataObject, new()
                where TServiceOutput : System.Runtime.Serialization.IExtensibleDataObject, new()
        {
            using (CallContext ct = new CallContext(
                this.Logger,
                request.EffectiveTrackingGuid,
                type))
            {
                TDataAccessResponse response = default(TDataAccessResponse);
                DataAccessorTracerResult tracerResult = new DataAccessorTracerResult();
                tracerResult.ApiName = type.ToString();
                tracerResult.TrackingGuid = request.EffectiveTrackingGuid;
                bool isSucceeded = false;

                try
                {
                    ct.AddInputParameter("RemoteUrl", this.remoteUrl);
                    ct.AddInputParameter("CertInformation", this.CertInformation);

                    ct.AddRichInputParameter(request.RequestName, request);
                    ct.Delegator = request.Delegater;
                    ct.Requestor = request.Requester;
                    ct.ObjectId = request.ObjectId;

                    tracerResult.StartWatch("InputToString");
                    tracerResult.ApiRequest = request.DataContractToString();
                    tracerResult.StopWatch("InputToString");

                    tracerResult.StartWatch("InputToRequest");
                    TServiceInput serviceInput = default(TServiceInput);
                    serviceInput = constructServiceInput(request);
                    tracerResult.StopWatch("InputToRequest");

                    tracerResult.StartWatch("RequestToString");
                    tracerResult.RawApiRequest = serviceInput.DataContractToString();
                    tracerResult.StopWatch("RequestToString");

                    tracerResult.ApiStart = DateTime.UtcNow;

                    TServiceOutput serviceOutput = default(TServiceOutput);
                    while (tracerResult.TryCount++ < SystemConstants.CommerceServiceMaxTryCount)
                    {
                        try
                        {
                            using (ChannelFactory<TServieChannel> clientFactory = new ChannelFactory<TServieChannel>(this.binding, this.address))
                            {
                                clientFactory.Endpoint.Contract.Name = ContractName;

                                if (clientFactory.Credentials.ClientCertificate.Certificate == null)
                                {
                                    clientFactory.Credentials.ClientCertificate.Certificate = this.clientCert;
                                }

                                using (TServieChannel channel = clientFactory.CreateChannel())
                                {
                                    serviceOutput = serviceExecute(channel, serviceInput);
                                }
                            }

                            tracerResult.ApiResponseType = DataAccessorTracerResult.ApiSuccess;

                            tracerResult.StartWatch("ResponseToString");
                            tracerResult.RawApiResponse = serviceOutput.DataContractToString();
                            tracerResult.StopWatch("ResponseToString");

                            tracerResult.ApiEnd = DateTime.UtcNow;
                            break;
                        }
                        catch (TimeoutException ex)
                        {
                            int trysRemaining = SystemConstants.CommerceServiceMaxTryCount - tracerResult.TryCount;
                            this.Logger.Warning(
                                "TimeoutException when calling {0}, {1} times remaining: {2}.",
                                type.ToString(),
                                trysRemaining,
                                ex.ToString());

                            if (trysRemaining == 0)
                            {
                                this.Logger.Exception(ex, "TimeoutException when calling {0}.", type.ToString());
                                throw new DataAccessException(
                                    DataAccessErrors.DATAACCESS_E_EXTERNAL_TIMEOUT_ERROR,
                                    string.Format("TimeoutException when calling {0}.", type.ToString()),
                                    ex, tracerResult);
                            }
                            else
                                Thread.Sleep(SystemConstants.CommerceServiceRetryInterval);
                        }
                        catch (Exception ex)
                        {
                            this.Logger.Exception(
                                ex,
                                "Exception when calling {0}.",
                                type.ToString());

                            throw new DataAccessException(
                                DataAccessErrors.DATAACCESS_E_SERVICECALL_ERROR,
                                string.Format("Exception when calling {0}.", type.ToString()),
                                ex, tracerResult);
                        }
                    }

                    tracerResult.StartWatch("ResponseToOutput");
                    response = constructDataAccessOutput(serviceOutput);
                    tracerResult.StopWatch("ResponseToOutput");

                    tracerResult.ApiResponse = response.DataContractToString();
                    response.TracerResult = tracerResult;
                    isSucceeded = true;

                    return response;
                }
                catch (DataAccessException te)
                {
                    tracerResult.SetException(te, te.ErrorCode);
                    tracerResult.ApiEnd = DateTime.UtcNow;
                    te.TracerResult = tracerResult;

                    ct.SetException(te, te.ErrorNamespace, te.ErrorCode);
                    this.Logger.Exception(te);

                    throw;
                }
                catch (Exception ex)
                {
                    tracerResult.SetException(ex, DataAccessErrors.DATAACCESS_E_INTERNAL_SERVER_ERROR);
                    tracerResult.ApiEnd = DateTime.UtcNow;

                    this.Logger.Exception(ex);
                    ct.SetException(ex);

                    throw new DataAccessException(
                        DataAccessErrors.DATAACCESS_E_INTERNAL_SERVER_ERROR,
                        string.Format("Exception when calling {0}.", type.ToString()),
                        ex, tracerResult);
                }
                finally
                {
                    if (response != null)
                    {
                        ct.AddRichOutputParameter(response.ResponseName, response);
                    }

                    string certInfo = string.Format("Client certificate Subject: '{0}' Issuer: '{1}' Thumbprint: '{2}'", this.clientCert == null ? "<none>" : this.clientCert.Subject, this.clientCert == null ? "<none>" : this.clientCert.Issuer, this.clientCert == null ? "<none>" : this.clientCert.Thumbprint);

                    TracePXServiceOutgoingOperation(
                        serviceName,
                        this.remoteUrl,
                        traceActivityId,
                        isSucceeded,
                        tracerResult,
                        certInfo);
                }
            }
        }

        private static void TracePXServiceOutgoingOperation(
            string serviceName,
            string remoteUrl,
            EventTraceActivity traceActivityId,
            bool isSucceeded,
            DataAccessorTracerResult tracerResult,
            string certInfo)
        {
            try
            {
                string correlationVector;
                if (traceActivityId.CorrelationVectorV4 == null || string.IsNullOrWhiteSpace(traceActivityId.CorrelationVectorV4.Value))
                {
                    correlationVector = new CorrelationVector().Value;
                }
                else
                {
                    correlationVector = traceActivityId.CorrelationVectorV4.Increment();
                }

                SllWebLogger.TracePXServiceOutgoingOperation(
                            operationName: tracerResult.ApiName,
                            serviceName: serviceName,
                            targetUri: remoteUrl,
                            requestPayload: tracerResult.RawApiRequest,
                            responsePayload: tracerResult.RawApiResponse,
                            startTime: tracerResult.ApiStart.ToString("o"),
                            latencyMs: Convert.ToInt32((tracerResult.ApiEnd - tracerResult.ApiStart).Milliseconds),
                            requestTraceId: traceActivityId.ActivityId.ToString(),
                            correlationVector: correlationVector,
                            isSucceeded: isSucceeded,
                            message: string.Format(
                                "Tracking Guid: {0}.  Try Count: {1}. Elapsed Times: {2}.",
                                tracerResult.TrackingGuid,
                                tracerResult.TryCount,
                                tracerResult.ElapsedTimes),
                            certInfo: certInfo);
            }
            catch (Exception ex)
            {
                SllWebLogger.TracePXServiceException("CommerceServiceExecutor.TracePXServiceOutgoingOperation: " + ex.ToString(), traceActivityId);
            }
        }
    }
}
