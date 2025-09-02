namespace Test.Common
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;

    public class MockServiceDelegatingHandlerV2 : DelegatingHandler, IMockService
    {
        public List<string> Requests => this.mockServiceHandler.Requests;

        public List<ConditionalResponse> Responses => this.mockServiceHandler.Responses;

        public Action<HttpRequestMessage> PreProcess
        {
            get
            {
                return this.mockServiceHandler.PreProcess;
            }

            set
            {
                this.mockServiceHandler.PreProcess = value;
            }
        }

        private MockServiceHandler mockServiceHandler;

        private readonly Dictionary<string, Func<HttpRequestMessage, bool>> routeMatchers;

        private readonly string responseProviderName;

        public MockServiceDelegatingHandlerV2(IMockResponseProvider mockResponseProvider, bool useArrangedResponses)
        {
            mockServiceHandler = new MockServiceHandler(mockResponseProvider, useArrangedResponses);
            responseProviderName = GetMockResponseProviderName(mockResponseProvider);

            routeMatchers = new Dictionary<string, Func<HttpRequestMessage, bool>>
            {
                { Constants.PXDependencyEmulatorsMockResponseProviders.PartnerSettings, req => Contains(req.RequestUri.AbsolutePath, "partnersettings") },

                { Constants.PXDependencyEmulatorsMockResponseProviders.PIMS, 
                    req => ContainsAny(
                        req.RequestUri.AbsolutePath, 
                        new[] 
                        { 
                            "paymentInstrument", "validatecvv", "paymentMethods", "Searchbyaccountnumber", "sessions", "eligiblePaymentMethods", "thirdPartyPayments"
                        })
                },
                { Constants.PXDependencyEmulatorsMockResponseProviders.MSRewards, req => Contains(req.RequestUri.AbsolutePath, "api/") && ContainsAny(req.RequestUri.AbsolutePath, new[] { "users", "orders", "?options" })
                },
                { Constants.PXDependencyEmulatorsMockResponseProviders.Account, req => ContainsAny(req.RequestUri.AbsolutePath, new[] { "/profiles", "/customers", "/addresses", "/get-or-create-legacy-billable-account" })
                },
                { Constants.PXDependencyEmulatorsMockResponseProviders.Catalog, req => ContainsAny(req.RequestUri.AbsolutePath, new[] { "/products", "eudirective" })
                },
                { Constants.PXDependencyEmulatorsMockResponseProviders.IssuerService, req => ContainsAny(req.RequestUri.AbsolutePath, new[] { "applications/", "eligibility" }) && !ContainsAny(req.RequestUri.AbsolutePath, new[] { "consumer", "accept" })
                },
                { Constants.PXDependencyEmulatorsMockResponseProviders.ChallengeManagement, req => ContainsAny(req.RequestUri.AbsolutePath, new[] { "challenge", "challengesession" })
                },
                { Constants.PXDependencyEmulatorsMockResponseProviders.Purchase, req => ContainsAny(req.RequestUri.AbsolutePath, new[] { "v8.0/users", "v7.0/users" }) && ContainsAny(req.RequestUri.AbsolutePath, new[] { "orders", "recurrences", "check" })
                },
                { Constants.PXDependencyEmulatorsMockResponseProviders.Risk, req => Contains(req.RequestUri.AbsolutePath, "risk/risk-evaluation")
                },
                { Constants.PXDependencyEmulatorsMockResponseProviders.TokenPolicy, req => Contains(req.RequestUri.AbsolutePath, "tokenDescriptionRequests")
                },
                { Constants.PXDependencyEmulatorsMockResponseProviders.StoredValue, req => ContainsAny(req.RequestUri.AbsolutePath, new[] { "gift-catalog", "/funds" })
                },
                { Constants.PXDependencyEmulatorsMockResponseProviders.SellerMarketPlace, req => Contains(req.RequestUri.AbsolutePath, "/sellers")
                },
                { Constants.PXDependencyEmulatorsMockResponseProviders.Wallet, req => Contains(req.RequestUri.AbsolutePath, "wallet")
                },
                { Constants.PXDependencyEmulatorsMockResponseProviders.TransactionData, req => Contains(req.RequestUri.AbsolutePath, "provision")
                },
                { Constants.PXDependencyEmulatorsMockResponseProviders.TransactionService, req => Contains(req.RequestUri.AbsolutePath, "/payments")
                },
                { Constants.PXDependencyEmulatorsMockResponseProviders.PaymentOrchestrator, req => Contains(req.RequestUri.AbsolutePath, "checkoutRequests/")
                },
                { 
                    Constants.PXDependencyEmulatorsMockResponseProviders.PayerAuth, req => ContainsAny(req.RequestUri.AbsolutePath, new[] { "CreatePaymentSessionId", "GetThreeDSMethodURL", "CompleteChallenge", "result", "Authenticate" })
                },
                { 
                    Constants.PXDependencyEmulatorsMockResponseProviders.FraudDetection, req => Contains(req.RequestUri.AbsolutePath, "/botcheck")
                }
            };
        }

        public void ArrangeResponse(
            string content,
            HttpStatusCode statusCode = HttpStatusCode.OK,
            HttpMethod method = null,
            string urlPattern = null)
        {
            this.mockServiceHandler.ArrangeResponse(content, statusCode, method, urlPattern);
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (ShouldHandleRequest(request))
            {
                return await this.mockServiceHandler.SendAsync(request, cancellationToken, (message, token) => base.SendAsync(message, token));
            }
            else
            {
                return await base.SendAsync(request, cancellationToken);
            }
        }

        private bool ShouldHandleRequest(HttpRequestMessage request)
        {
            if (routeMatchers.TryGetValue(responseProviderName, out var matcher))
            {
                return matcher(request);
            }

            return true;
        }

        private string GetMockResponseProviderName(IMockResponseProvider mockResponseProvider)
        {
            return mockResponseProvider.GetType().Name;
        }

        private static bool Contains(string source, string value)
        {
            return source.IndexOf(value, StringComparison.OrdinalIgnoreCase) > 0;
        }

        private static bool ContainsAny(string source, string[] values)
        {
            return values.Any(value => source.IndexOf(value, StringComparison.OrdinalIgnoreCase) > 0);
        }
    }
}