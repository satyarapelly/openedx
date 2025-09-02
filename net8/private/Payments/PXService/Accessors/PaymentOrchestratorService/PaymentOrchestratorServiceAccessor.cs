// <copyright file="PaymentOrchestratorServiceAccessor.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Accessors.PaymentOrchestratorService
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Text;
    using System.Threading.Tasks;
    using Common;
    using Common.Tracing;
    using Common.Web;
    using Microsoft.Commerce.Payments.PimsModel.V4;
    using Microsoft.Commerce.Payments.PXService.Model.PaymentOrchestratorService;
    using Microsoft.Commerce.Payments.PXService.V7.PaymentChallenge.Model;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using PXCommon;
    using Address = Model.PaymentOrchestratorService.Address;

    /// <summary>
    /// Provides functionality to access PaymentOrchestratorService
    /// </summary>
    public class PaymentOrchestratorServiceAccessor : IPaymentOrchestratorServiceAccessor
    {
        private const string ApiVersionHeaderName = "x-ms-api-version";
        private const string TraceParentHeaderName = "traceparent";

        private readonly List<string> passThroughHeaders = new List<string>
        {
            PaymentConstants.PaymentExtendedHttpHeaders.TestHeader,
            GlobalConstants.HeaderValues.XMsRequestContext
        };

        private string serviceBaseUrl;
        private string emulatorBaseUrl;

        private string apiVersion;
        private HttpClient paymentOrchestratorServiceHttpClient;

        public PaymentOrchestratorServiceAccessor(
            string serviceBaseUrl,
            string emulatorBaseUrl,
            string apiVersion,
            HttpMessageHandler messageHandler)
        {
            this.serviceBaseUrl = serviceBaseUrl;
            this.emulatorBaseUrl = emulatorBaseUrl;
            this.apiVersion = apiVersion;

            this.paymentOrchestratorServiceHttpClient = new PXTracingHttpClient(PXCommon.Constants.ServiceNames.PaymentOrchestratorService, messageHandler);
            this.paymentOrchestratorServiceHttpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(PaymentConstants.HttpMimeTypes.JsonContentType));
            this.paymentOrchestratorServiceHttpClient.DefaultRequestHeaders.Add(PaymentConstants.HttpHeaders.Connection, PaymentConstants.HttpHeaders.KeepAlive);
            this.paymentOrchestratorServiceHttpClient.DefaultRequestHeaders.Add(PaymentConstants.HttpHeaders.KeepAlive, string.Format(PaymentConstants.HttpHeaders.KeepAliveParameter, 60));
        }

        private string BaseUrl
        {
            get
            {
                if (HttpRequestHelper.IsPXTestRequest() && !string.IsNullOrWhiteSpace(this.emulatorBaseUrl))
                {
                    return this.emulatorBaseUrl;
                }
                else
                {
                    return this.serviceBaseUrl;
                }
            }
        }

        /// <summary>
        /// Get the payment request
        /// </summary>
        /// <param name="paymentRequestId">Payment Request Id</param>
        /// <param name="traceActivityId">Trace Activity Id</param>
        /// <returns>Payment request</returns>
        public async Task<PaymentRequest> GetPaymentRequest(string paymentRequestId, EventTraceActivity traceActivityId)
        {
            string requestUrl = string.Format(V7.Constants.UriTemplate.PaymentOrchestratorServiceGetPaymentRequest, paymentRequestId);
            var additionalHeaders = this.CreateStandardHeaders(traceActivityId);
            
            return await this.SendRequest<PaymentRequest>(
                HttpMethod.Get,
                requestUrl,
                null,
                "GetPaymentRequest",
                traceActivityId,
                additionalHeaders);
        }

        /// <summary>
        /// Confirm the payment request
        /// </summary>
        /// <param name="paymentRequestId">Payment request Id</param>
        /// <param name="traceActivityId">Trace Activity Id</param>        
        /// <returns>Attach Payment Instrument Response</returns>        
        public async Task<PaymentRequestClientActions> PaymentRequestConfirm(string paymentRequestId, EventTraceActivity traceActivityId)
        {
            string requestUrl = string.Format(V7.Constants.UriTemplate.PaymentOrchestratorServiceConfirmPaymentRequest, paymentRequestId);
            string payload = "{}"; // Empty payload for confirm payment request

            var additionalHeaders = this.CreateStandardHeaders(traceActivityId);

            return await this.SendRequest<PaymentRequestClientActions>(
                HttpMethod.Post,
                requestUrl,
                payload,
                "PaymentRequestConfirm",
                traceActivityId,
                additionalHeaders);
        }

        /// <summary>
        /// Attaches the challenge data to the payment request
        /// </summary>
        /// <param name="paymentRequestId">Payment Request Id</param>
        /// <param name="piId">Payment Instrument Id</param>
        /// <param name="challengeType">challenge type</param>/// 
        /// <param name="challengeValue">challenge value like Cvv Token</param>
        /// <param name="traceActivityId">Trace activity Id</param>
        /// <returns>Payment Request Client Actions</returns>
        public async Task<PaymentRequestClientActions> PaymentRequestAttachChallengeData(string paymentRequestId, string piId, PaymentInstrumentChallengeType challengeType, string challengeValue, EventTraceActivity traceActivityId)
        {
            var paymenRequest = await this.GetPaymentRequest(paymentRequestId, traceActivityId);
            
            if (paymenRequest == null)
            {
                throw new Exception($"Failed to get the payment request from Payment Orchestrator Service for requestId {paymentRequestId}");
            }

            if (paymenRequest.PaymentInstruments == null || paymenRequest.PaymentInstruments.Count <= 0)
            {
                throw new Exception($"No payment instruments found in the payment request from Payment Orchestrator Service for requestId {paymentRequestId}");
            }

            var paymentInstrument = paymenRequest.PaymentInstruments.FirstOrDefault(pi => pi.PaymentInstrumentId == piId);
            if (paymentInstrument == null)
            {
                throw new Exception($"The PiId provided doesn't match with payment instruments attached to the payment request from Payment Orchestrator Service for requestId {paymentRequestId}");
            }

            string requestUrl = string.Format(V7.Constants.UriTemplate.PaymentOrchestratorServiceAttachChallengeData, paymentRequestId);
            var attachRequest = new AttachChallengeDataContext()
            {
                PaymentInstrumentId = paymentInstrument.PaymentInstrumentId,
                ChallengeData = new ChallengeData() { ChallengeType = challengeType, ChallengeValue = challengeValue }
            };

            var additionalHeaders = this.CreateStandardHeaders(traceActivityId);

            return await this.SendRequest<PaymentRequestClientActions>(
                HttpMethod.Post,
                requestUrl,
                attachRequest,
                "PaymentRequestAttachChallengeData",
                traceActivityId,
                additionalHeaders);
        }

        /// <summary>
        /// Remove eligible payment methods from payment request
        /// </summary>
        /// <param name="paymentRequestId">Payment Request Id</param>
        /// <param name="piId">Payment Instrument Id</param>
        /// <param name="traceActivityId">Trace activity Id</param>
        /// <returns>Payment Request Client Actions</returns>
        public async Task<PaymentRequestClientActions> RemoveEligiblePaymentmethods(string paymentRequestId, string piId, EventTraceActivity traceActivityId)
        {
            string requestUrl = string.Format(V7.Constants.UriTemplate.PaymentOrchestratorServiceRemoveEligiblePaymentmethods, paymentRequestId);
            var piRemoveRequest = new RemoveEligiblePaymentMethodsContext()
            {
                PIID = piId,
            };

            var additionalHeaders = this.CreateStandardHeaders(traceActivityId);

            return await this.SendRequest<PaymentRequestClientActions>(
                HttpMethod.Post,
                requestUrl,
                piRemoveRequest,
                "Removeeligiblepaymentmethods",
                traceActivityId,
                additionalHeaders);
        }

        /// <summary>
        /// Attaches the challenge data to the payment request
        /// TODO: delete this method after pr and cr are merged 
        /// </summary>
        /// <param name="requestId">Payment Request Id</param>
        /// <param name="piId">Payment Instrument Id</param>
        /// <param name="challengeType">challenge type</param> 
        /// <param name="challengeStatus">challenge status</param>
        /// <param name="paymentSessionId">payment SessionId</param>
        /// <param name="traceActivityId">Trace activity Id</param>
        /// <param name="tenantId">Tenant Id</param>
        /// <returns>Payment Request Client Actions</returns>
        public async Task<object> PSD2AttachChallengeData(string requestId, string piId, PaymentInstrumentChallengeType challengeType, PaymentChallengeStatus challengeStatus, string paymentSessionId, EventTraceActivity traceActivityId, string tenantId)
        {
            const string PurchaseRequestPrefix = "pr_";
            const string CheckoutRequestPrefix = "cr_";

            string requestUrl = string.Empty;

            var attachRequest = new AttachPSD2ChallengeDataContext()
            {
                PaymentInstrumentId = piId,
                ChallengeData = new Psd2ChallengeData()
                {
                    ChallengeType = challengeType,
                    ChallengeValue = new Psd2ChallengeValue()
                    {
                        ChallengeStatus = challengeStatus,
                        PaymentSessionId = paymentSessionId
                    }
                }
            };

            var additionalHeaders = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>(ApiVersionHeaderName, this.apiVersion),
                new KeyValuePair<string, string>(TraceParentHeaderName, traceActivityId.CorrelationVectorV4.Value),
                new KeyValuePair<string, string>(GlobalConstants.HeaderValues.XMsRequestContext, string.Format(V7.Constants.RequestHeaderValueTemplate.RequestContext, tenantId)),
            };

            if (requestId.StartsWith(PurchaseRequestPrefix, StringComparison.OrdinalIgnoreCase))
            {
                requestUrl = string.Format(V7.Constants.UriTemplate.PaymentOrchestratorServiceAttachChallengeData, requestId);

                return await this.SendRequest<PaymentRequestClientActions>(
                    HttpMethod.Post,
                    requestUrl,
                    attachRequest,
                    "PSD2AttachChallengeData",
                    traceActivityId,
                    additionalHeaders);
            }
            else if (requestId.StartsWith(CheckoutRequestPrefix, StringComparison.OrdinalIgnoreCase))
            {
                requestUrl = string.Format(V7.Constants.UriTemplate.PaymentOrchestratorServiceAttachCheckoutRequestChallengeData, requestId);

                return await this.SendRequest<CheckoutRequestClientActions>(
                    HttpMethod.Post,
                    requestUrl,
                    attachRequest,
                    "PSD2AttachChallengeData",
                    traceActivityId,
                    additionalHeaders);
            }
            else
            {
                throw new ArgumentException("Invalid request id");
            }
        }

        /// <summary>
        /// Attaches the challenge data to the payment request
        /// </summary>
        /// <param name="requestId">Payment Request Id</param>
        /// <param name="piId">Payment Instrument Id</param>
        /// <param name="challengeType">challenge type</param> 
        /// <param name="challengeStatus">challenge status</param>
        /// <param name="paymentSessionId">payment SessionId</param>
        /// <param name="traceActivityId">Trace activity Id</param>
        /// <param name="tenantId">Tenant Id</param>
        /// <returns>Payment Request Client Actions</returns>
        public async Task<PaymentRequestClientActions> PSD2AttachChallengeDataToPaymentRequest(string requestId, string piId, PaymentInstrumentChallengeType challengeType, PaymentChallengeStatus challengeStatus, string paymentSessionId, EventTraceActivity traceActivityId, string tenantId)
        {
            string requestUrl = string.Format(V7.Constants.UriTemplate.PaymentOrchestratorServiceAttachChallengeData, requestId);

            var attachRequest = new AttachPSD2ChallengeDataContext()
            {
                PaymentInstrumentId = piId,
                ChallengeData = new Psd2ChallengeData()
                {
                    ChallengeType = challengeType,
                    ChallengeValue = new Psd2ChallengeValue()
                    {
                        ChallengeStatus = challengeStatus,
                        PaymentSessionId = paymentSessionId
                    }
                }
            };

            var additionalHeaders = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>(ApiVersionHeaderName, this.apiVersion),
                new KeyValuePair<string, string>(TraceParentHeaderName, traceActivityId.CorrelationVectorV4.Value),
                new KeyValuePair<string, string>(GlobalConstants.HeaderValues.XMsRequestContext, string.Format(V7.Constants.RequestHeaderValueTemplate.RequestContext, tenantId)),
            };

            return await this.SendRequest<PaymentRequestClientActions>(
                HttpMethod.Post,
                requestUrl,
                attachRequest,
                "PSD2AttachChallengeData",
                traceActivityId,
                additionalHeaders);
        }

        /// <summary>
        /// Attaches the payment instrument to the appropriate object
        /// TODO: delete this method after pr and cr are merged 
        /// </summary>
        /// <param name="requestId">Request Id</param>
        /// <param name="paymentInstrumentId">Payment Instrument Id</param>
        /// <param name="cvvToken">Cvv Token</param>
        /// <param name="traceActivityId">Trace Activity Id</param>
        /// <param name="savePaymentDetails">Should save payment instrument as onfile usage type in PO</param>
        /// <returns>Attach Payment Instrument Response</returns>
        public async Task<AttachPaymentInstrumentResponse> AttachPaymentInstrument(string requestId, string paymentInstrumentId, string cvvToken, EventTraceActivity traceActivityId, string savePaymentDetails)
        {
            const string PurchaseRequestPrefix = "pr_";
            const string WalletRequestPrefix = "wr_";
            const string CheckoutRequestPrefix = "cr_";
            const string MethodName = "AttachPaymentInstrument";

            AttachPaymentsInstrumentsContext attachRequest = new AttachPaymentsInstrumentsContext()
            {
                PaymentInstruments = new List<PaymentInstrumentContext>() { new PaymentInstrumentContext { PaymentInstrumentId = paymentInstrumentId, Usage = PaymentInstrumentUsage.PrimaryPaymentInstrument, TransactionData = new TransactionData { DataSchema = PaymentInstrumentChallengeType.Cvv, DataValue = cvvToken } } }
            };

            var additionalHeaders = this.CreateStandardHeaders(traceActivityId);

            var returnValue = new AttachPaymentInstrumentResponse() { RequestId = requestId };

            if (requestId.StartsWith(PurchaseRequestPrefix, StringComparison.OrdinalIgnoreCase))
            {
                string requestUrl = string.Format(V7.Constants.UriTemplate.PaymentOrchestratorServiceAttachPaymentInstrument, requestId);

                var response = await this.SendRequest<PaymentRequest>(
                    HttpMethod.Post,
                    requestUrl,
                    attachRequest,
                    MethodName,
                    traceActivityId,
                    additionalHeaders);

                returnValue.Status = response.Status.ToString();

                return returnValue;
            }
            else if (requestId.StartsWith(WalletRequestPrefix, StringComparison.OrdinalIgnoreCase))
            {
                string requestUrl = string.Format(V7.Constants.UriTemplate.PaymentOrchestratorServiceAttachPaymentInstrumentWallet, requestId);

                var response = await this.SendRequest<WalletRequest>(
                    HttpMethod.Post,
                    requestUrl,
                    attachRequest,
                    MethodName,
                    traceActivityId,
                    additionalHeaders);

                returnValue.Status = response.Status.ToString();
                return returnValue;
            }
            else if (requestId.StartsWith(CheckoutRequestPrefix, StringComparison.OrdinalIgnoreCase))
            {
                string requestUrl = string.Format(V7.Constants.UriTemplate.PaymentOrchestratorServiceAttachPaymentInstrumentToCheckoutRequest, requestId);
                AttachPaymentsInstrumentsContextForCheckoutRequest attachRequestForCheckoutRequest = new AttachPaymentsInstrumentsContextForCheckoutRequest()
                {
                    PaymentInstruments = new List<PaymentInstrumentContext>()
                    {
                        new PaymentInstrumentContext
                        {
                            PaymentInstrumentId = paymentInstrumentId,
                            Usage = PaymentInstrumentUsage.PrimaryPaymentInstrument,
                            TransactionData = new TransactionData { DataSchema = PaymentInstrumentChallengeType.Cvv, DataValue = cvvToken },
                            ActionAfterInitialTransaction = string.Equals(savePaymentDetails, "True", StringComparison.OrdinalIgnoreCase) ? PaymentInstrumentActionType.VaultOnSuccess : PaymentInstrumentActionType.None,
                        }
                    }
                };

                var response = await this.SendRequest<WalletRequest>(
                    HttpMethod.Post,
                    requestUrl,
                    attachRequestForCheckoutRequest,
                    MethodName,
                    traceActivityId,
                    additionalHeaders);

                returnValue.Status = response.Status.ToString();
                return returnValue;
            }
            else
            {
                throw new ArgumentException("Invalid request id");
            }
        }

        /// <summary>
        /// Attaches the payment instrument to the appropriate object
        /// </summary>
        /// <param name="requestId">Request Id</param>
        /// <param name="paymentInstrumentId">Payment Instrument Id</param>
        /// <param name="cvvToken">Cvv Token</param>
        /// <param name="traceActivityId">Trace Activity Id</param>
        /// <param name="savePaymentDetails">Should save payment instrument as onfile usage type in PO</param>
        /// <returns>Attach Payment Instrument Response</returns>
        public async Task<AttachPaymentInstrumentResponse> AttachPaymentInstrumentToPaymentRequest(string requestId, string paymentInstrumentId, string cvvToken, EventTraceActivity traceActivityId, string savePaymentDetails)
        {
            const string WalletRequestPrefix = "wr_";
            const string MethodName = "AttachPaymentInstrument";

            AttachPaymentsInstrumentsContext attachRequest = new AttachPaymentsInstrumentsContext()
            {
                PaymentInstruments = new List<PaymentInstrumentContext>() { new PaymentInstrumentContext { PaymentInstrumentId = paymentInstrumentId, Usage = PaymentInstrumentUsage.PrimaryPaymentInstrument, TransactionData = new TransactionData { DataSchema = PaymentInstrumentChallengeType.Cvv, DataValue = cvvToken } } }
            };

            var additionalHeaders = this.CreateStandardHeaders(traceActivityId);

            var returnValue = new AttachPaymentInstrumentResponse() { RequestId = requestId };

            if (requestId.StartsWith(WalletRequestPrefix, StringComparison.OrdinalIgnoreCase))
            {
                string requestUrl = string.Format(V7.Constants.UriTemplate.PaymentOrchestratorServiceAttachPaymentInstrumentWallet, requestId);

                var response = await this.SendRequest<WalletRequest>(
                    HttpMethod.Post,
                    requestUrl,
                    attachRequest,
                    MethodName,
                    traceActivityId,
                    additionalHeaders);

                returnValue.Status = response.Status.ToString();

                return returnValue;
            }
            else
            {
                string requestUrl = string.Format(V7.Constants.UriTemplate.PaymentOrchestratorServiceAttachPaymentInstrument, requestId);

                AttachPaymentsInstrumentsContextForCheckoutRequest attachRequestForCheckoutRequest = new AttachPaymentsInstrumentsContextForCheckoutRequest()
                {
                    PaymentInstruments = new List<PaymentInstrumentContext>()
                    {
                        new PaymentInstrumentContext
                        {
                            PaymentInstrumentId = paymentInstrumentId,
                            Usage = PaymentInstrumentUsage.PrimaryPaymentInstrument,
                            TransactionData = new TransactionData { DataSchema = PaymentInstrumentChallengeType.Cvv, DataValue = cvvToken },
                            ActionAfterInitialTransaction = string.Equals(savePaymentDetails, "True", StringComparison.OrdinalIgnoreCase) ? PaymentInstrumentActionType.VaultOnSuccess : PaymentInstrumentActionType.None,
                        }
                    }
                };

                var response = await this.SendRequest<PaymentRequest>(
                    HttpMethod.Post,
                    requestUrl,
                    attachRequestForCheckoutRequest,
                    MethodName,
                    traceActivityId,
                    additionalHeaders);

                returnValue.Status = response.Status.ToString();
                return returnValue;
            }
        }

        /// <summary>
        /// Attaches the address to the checkout request
        /// TODO: delete this method after pr and cr are merged
        /// </summary>
        /// <param name="address"> Payment Rerquest Id</param>
        /// <param name="type">Payment Instrument Id</param>
        /// <param name="traceActivityId">Trace Activity Id</param>
        /// <param name="checkoutRequestId">Checkout request Id</param>
        /// <returns>Attach Payment Instrument Response</returns>
        public async Task<CheckoutRequestClientActions> AttachAddress(Address address, string type, EventTraceActivity traceActivityId, string checkoutRequestId)
        {
            string requestUrl = string.Format(V7.Constants.UriTemplate.PaymentOrchestratorServiceAttachAddressToCheckoutRequest, checkoutRequestId, type);
            AttachAddressContext attachAddressContext = new AttachAddressContext()
            {
                Address = address
            };

            var additionalHeaders = this.CreateStandardHeaders(traceActivityId);

            return await this.SendRequest<CheckoutRequestClientActions>(
                HttpMethod.Post,
                requestUrl,
                attachAddressContext,
                "AttachAddress",
                traceActivityId,
                additionalHeaders);
        }

        /// <summary>
        /// Attaches the address to the payment request
        /// </summary>
        /// <param name="address"> Payment Rerquest Id</param>
        /// <param name="type">Payment Instrument Id</param>
        /// <param name="traceActivityId">Trace Activity Id</param>
        /// <param name="paymentRequestId">Payment request Id</param>
        /// <returns>Attach Payment Instrument Response</returns>
        public async Task<PaymentRequestClientActions> AttachAddressToPaymentRequest(Address address, string type, EventTraceActivity traceActivityId, string paymentRequestId)
        {
            string requestUrl = string.Format(V7.Constants.UriTemplate.PaymentOrchestratorServiceAttachAddressToPaymentRequest, paymentRequestId, type);
            AttachAddressContext attachAddressContext = new AttachAddressContext()
            {
                Address = address
            };

            var additionalHeaders = this.CreateStandardHeaders(traceActivityId);

            return await this.SendRequest<PaymentRequestClientActions>(
                HttpMethod.Post,
                requestUrl,
                attachAddressContext,
                "AttachAddress",
                traceActivityId,
                additionalHeaders);
        }

        /// <summary>
        /// Attaches the profile to the checkout request
        /// TODO: delete this method after pr and cr are merged
        /// </summary>
        /// <param name="email">email to be attached</param>
        /// <param name="traceActivityId">Trace Activity Id</param>
        /// <param name="checkoutRequestId">Checkout request Id</param>
        /// <returns>Attach Payment Instrument Response</returns>
        public async Task<CheckoutRequestClientActions> AttachProfile(string email, EventTraceActivity traceActivityId, string checkoutRequestId)
        {
            string requestUrl = string.Format(V7.Constants.UriTemplate.PaymentOrchestratorServiceAttachProfileToCheckoutRequest, checkoutRequestId);

            var additionalHeaders = this.CreateStandardHeaders(traceActivityId);

            AttachProfileContext attachProfileContext = new AttachProfileContext()
            {
                Profile = new CustomerProfile()
                {
                    Email = email
                }
            };

            return await this.SendRequest<CheckoutRequestClientActions>(
                HttpMethod.Post,
                requestUrl,
                attachProfileContext,
                "AttachProfile",
                traceActivityId,
                additionalHeaders);
        }

        /// <summary>
        /// Attaches the profile to the checkout request
        /// </summary>
        /// <param name="email">email to be attached</param>
        /// <param name="traceActivityId">Trace Activity Id</param>
        /// <param name="paymentRequestId">Checkout request Id</param>
        /// <returns>Attach Payment Instrument Response</returns>
        public async Task<PaymentRequestClientActions> AttachProfileToPaymentRequest(string email, EventTraceActivity traceActivityId, string paymentRequestId)
        {
            string requestUrl = string.Format(V7.Constants.UriTemplate.PaymentOrchestratorServiceAttachProfileToPaymentRequest, paymentRequestId);

            var additionalHeaders = this.CreateStandardHeaders(traceActivityId);

            AttachProfileContext attachProfileContext = new AttachProfileContext()
            {
                Profile = new CustomerProfile()
                {
                    Email = email
                }
            };

            return await this.SendRequest<PaymentRequestClientActions>(
                HttpMethod.Post,
                requestUrl,
                attachProfileContext,
                "AttachProfile",
                traceActivityId,
                additionalHeaders);
        }

        /// <summary>
        /// Get client action from PO
        /// TODO: delete this method after pr and cr are merged
        /// </summary>
        /// <param name="traceActivityId">Trace Activity Id</param>
        /// <param name="checkoutRequestId">Checkout request Id</param>
        /// <returns>Attach Payment Instrument Response</returns>
        public async Task<CheckoutRequestClientActions> GetClientAction(EventTraceActivity traceActivityId, string checkoutRequestId)
        {
            string requestUrl = string.Format(V7.Constants.UriTemplate.PaymentOrchestratorServiceGetClientAction, checkoutRequestId);

            var additionalHeaders = this.CreateStandardHeaders(traceActivityId);

            return await this.SendRequest<CheckoutRequestClientActions>(
                HttpMethod.Get,
                requestUrl,
                null,
                "ClientAction",
                traceActivityId,
                additionalHeaders);
        }

        /// <summary>
        /// Get client action from PO
        /// </summary>
        /// <param name="traceActivityId">Trace Activity Id</param>
        /// <param name="paymentRequestId">Checkout request Id</param>
        /// <returns>Attach Payment Instrument Response</returns>
        public async Task<PaymentRequestClientActions> GetClientActionForPaymentRequest(EventTraceActivity traceActivityId, string paymentRequestId)
        {
            string requestUrl = string.Format(V7.Constants.UriTemplate.PaymentOrchestratorServiceGetPaymentRequestClientAction, paymentRequestId);

            var additionalHeaders = this.CreateStandardHeaders(traceActivityId);

            return await this.SendRequest<PaymentRequestClientActions>(
                HttpMethod.Get,
                requestUrl,
                null,
                "ClientAction",
                traceActivityId,
                additionalHeaders);
        }

        /// <summary>
        /// confirm to the checkout request
        /// TODO: delete this method after pr and cr are merged
        /// </summary>
        /// <param name="piid"> Payment Rerquest Id</param>
        /// <param name="traceActivityId">Trace Activity Id</param>
        /// <param name="checkoutRequestId">Checkout request Id</param>
        /// <returns>Attach Payment Instrument Response</returns>
        public async Task<CheckoutRequestClientActions> Confirm(string piid, EventTraceActivity traceActivityId, string checkoutRequestId)
        {
            string requestUrl = string.Format(V7.Constants.UriTemplate.PaymentOrchestratorServiceConfirmCheckoutRequest, checkoutRequestId);

            ConfirmContext attachProfileContext = new ConfirmContext()
            {
                PaymentInstruments = new List<PaymentInstrumentContext>()
                {
                    new PaymentInstrumentContext
                    {
                        PaymentInstrumentId = piid,
                        Usage = PaymentInstrumentUsage.PrimaryPaymentInstrument
                    }
                }
            };

            var additionalHeaders = this.CreateStandardHeaders(traceActivityId);

            return await this.SendRequest<CheckoutRequestClientActions>(
                HttpMethod.Post,
                requestUrl,
                attachProfileContext,
                "Confirm",
                traceActivityId,
                additionalHeaders);
        }

        /// <summary>
        /// confirm to the payment request used in candy crush flow
        /// </summary>
        /// <param name="piid"> Payment Rerquest Id</param>
        /// <param name="traceActivityId">Trace Activity Id</param>
        /// <param name="paymentRequestId">Payment request Id</param>
        /// <returns>Attach Payment Instrument Response</returns>
        public async Task<PaymentRequestClientActions> ConfirmToPaymentRequest(string piid, EventTraceActivity traceActivityId, string paymentRequestId)
        {
            string requestUrl = string.Format(V7.Constants.UriTemplate.PaymentOrchestratorServiceConfirmPaymentRequest, paymentRequestId);

            ConfirmContext attachProfileContext = new ConfirmContext()
            {
                PaymentInstruments = new List<PaymentInstrumentContext>()
                {
                    new PaymentInstrumentContext
                    {
                        PaymentInstrumentId = piid,
                        Usage = PaymentInstrumentUsage.PrimaryPaymentInstrument
                    }
                }
            };

            var additionalHeaders = this.CreateStandardHeaders(traceActivityId);

            return await this.SendRequest<PaymentRequestClientActions>(
                HttpMethod.Post,
                requestUrl,
                attachProfileContext,
                "Confirm",
                traceActivityId,
                additionalHeaders);
        }

        /// <summary>
        /// Get eligible payment methods from payment orchestrator service
        /// </summary>
        /// <param name="traceActivityId">Trace Activity Id</param>
        /// <param name="requestId">Request Id</param>
        /// <returns>List of eligible payment methods</returns>
        public async Task<WalletEligiblePaymentMethods> GetEligiblePaymentMethods(EventTraceActivity traceActivityId, string requestId)
        {
            string requestUrl = string.Format(V7.Constants.UriTemplate.PaymentOrchestratorServiceGetEligiblePaymentMethods, requestId);

            var additionalHeaders = this.CreateStandardHeaders(traceActivityId);

            return await this.SendRequest<WalletEligiblePaymentMethods>(
                HttpMethod.Get,
                requestUrl,
                null,
                "GetEligiblePaymentMethods",
                traceActivityId,
                additionalHeaders);
        }

        private static void AddHeaders(HttpRequestMessage request, IList<KeyValuePair<string, string>> headers)
        {
            if (headers != null && request != null)
            {
                foreach (var header in headers)
                {
                    if (string.Equals(header.Key, GlobalConstants.HeaderValues.ExtendedFlightName, StringComparison.OrdinalIgnoreCase))
                    {
                        // Adding duplicate HTTP headers is concatinating values with a comma and a space.  Downstream PIMS service is not parsing 
                        // the additonal space to identify the flights.
                        string existingFlightValue = request.GetRequestHeader(GlobalConstants.HeaderValues.ExtendedFlightName);
                        string additionalFlightValue = header.Value;
                        string newFlightValue = GetNewFlightValue(existingFlightValue, additionalFlightValue);

                        if (!string.IsNullOrWhiteSpace(newFlightValue))
                        {
                            request.Headers.Remove(header.Key);
                            request.Headers.Add(header.Key, newFlightValue);
                        }
                    }
                    else if (string.Equals(header.Key, GlobalConstants.HeaderValues.XMsRequestContext, StringComparison.OrdinalIgnoreCase))
                    {
                        // X-Ms-Request-Context header is passed from PIFD in nonanonymous request. Only add this header for anonymous request which doesn't contain this header.
                        if (!request.Headers.Contains(GlobalConstants.HeaderValues.XMsRequestContext))
                        {
                            request.Headers.Add(header.Key, header.Value);
                        }
                    }
                    else
                    {
                        request.Headers.Add(header.Key, header.Value);
                    }
                }
            }
        }

        private static string GetNewFlightValue(string existingFlightValue, string additionalFlightValue)
        {
            string newFlightValue = null;

            if (string.IsNullOrWhiteSpace(existingFlightValue))
            {
                if (!string.IsNullOrWhiteSpace(additionalFlightValue))
                {
                    newFlightValue = additionalFlightValue;
                }
            }
            else if (string.IsNullOrWhiteSpace(additionalFlightValue))
            {
                newFlightValue = existingFlightValue;
            }
            else
            {
                newFlightValue = string.Join(",", existingFlightValue, additionalFlightValue);
            }

            return newFlightValue;
        }

        /// <summary>
        /// Creates standard platform headers for requests to Payment Orchestrator Service
        /// </summary>
        /// <param name="traceActivityId">Trace Activity Id</param>
        /// <returns>List of key-value pairs containing standard headers</returns>
        private List<KeyValuePair<string, string>> CreateStandardHeaders(EventTraceActivity traceActivityId)
        {
            return new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>(ApiVersionHeaderName, this.apiVersion),
                new KeyValuePair<string, string>(TraceParentHeaderName, traceActivityId.CorrelationVectorV4.Value)
            };
        }

        private async Task<T> SendRequest<T>(HttpMethod method, string url, object request, string actionName, EventTraceActivity traceActivityId, IList<KeyValuePair<string, string>> additionalHeaders = null)
        {
            string fullRequestUrl = string.Format("{0}/{1}", this.BaseUrl, url);

            using (HttpRequestMessage requestMessage = new HttpRequestMessage(method, fullRequestUrl))
            {
                requestMessage.IncrementCorrelationVector(traceActivityId);
                requestMessage.Headers.Add(PaymentConstants.PaymentExtendedHttpHeaders.CorrelationId, traceActivityId.ActivityId.ToString());
                requestMessage.Headers.Add(PaymentConstants.PaymentExtendedHttpHeaders.TrackingId, Guid.NewGuid().ToString());
                requestMessage.Headers.Add(PaymentConstants.PaymentExtendedHttpHeaders.ApiVersion, this.apiVersion);

                HttpRequestHelper.TransferTargetHeadersFromIncomingRequestToOutgoingRequest(this.passThroughHeaders, requestMessage);

                // Add action name to the request properties so that this request's OperationName is logged properly
                requestMessage.AddOrReplaceActionName(actionName);

                AddHeaders(requestMessage, additionalHeaders);

                if (request != null)
                {
                    requestMessage.Content = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, PaymentConstants.HttpMimeTypes.JsonContentType);
                }

                using (HttpResponseMessage response = await this.paymentOrchestratorServiceHttpClient.SendAsync(requestMessage))
                {
                    string responseMessage = await response.Content.ReadAsStringAsync();

                    if (response.IsSuccessStatusCode)
                    {
                        try
                        {
                            return JsonConvert.DeserializeObject<T>(responseMessage);
                        }
                        catch (JsonException jsonEx)
                        {
                            SllWebLogger.TracePXServiceException("Failed to deserialize success response from {Constants.ServiceNames.PaymentOrchestratorService}: " + jsonEx.ToString(), traceActivityId);
                            throw TraceCore.TraceException(traceActivityId, new FailedOperationException($"Failed to deserialize success response from {Constants.ServiceNames.PaymentOrchestratorService}. Response Message: {responseMessage}", jsonEx));
                        }
                    }
                    else
                    {
                        if (string.IsNullOrWhiteSpace(responseMessage))
                        {
                            AccessorHandler.HandleEmptyErrorResponses(response, actionName, traceActivityId, PXCommon.Constants.ServiceNames.PaymentOrchestratorService);
                        }

                        ServiceErrorResponse error = null;
                        try
                        {
                            ServiceErrorResponse innerError = JsonConvert.DeserializeObject<ServiceErrorResponse>(responseMessage);
                            innerError.Source = string.IsNullOrWhiteSpace(innerError.Source) ? Constants.ServiceNames.PaymentOrchestratorService : innerError.Source;
                            error = new ServiceErrorResponse(traceActivityId.ActivityId.ToString(), GlobalConstants.ServiceName, innerError);
                        }
                        catch (Exception ex)
                        {
                            throw TraceCore.TraceException(traceActivityId, new FailedOperationException($"Failed to deserialize error response from {Constants.ServiceNames.PaymentOrchestratorService}. Response Message: {responseMessage}", ex));
                        }

                        throw TraceCore.TraceException(traceActivityId, new ServiceErrorResponseException() { Error = error, Response = response, HandlingType = ExceptionHandlingPolicy.ByPass });
                    }
                }
            }
        }
    }
}