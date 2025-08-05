// <copyright file="OrchestrationService.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace SelfHostedPXServiceCore.Mocks
{
    using System.Linq;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Commerce.Payments.Common.Tracing;
    using Microsoft.Commerce.Payments.PXService;
    using Microsoft.Commerce.Payments.PXService.Accessors.OrchestrationService.DataModel;
    using Newtonsoft.Json;
    using Test.Common;

    public class OrchestrationService : MockServiceWebRequestHandler
    {
        public OrchestrationService(OrchestrationServiceMockResponseProvider resolver, bool useArrangedResponses) : base(resolver, useArrangedResponses)
        {
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Requests?.Add(string.Format("{0} {1}", request.Method, request.RequestUri));

            if (PreProcess != null)
            {
                PreProcess(request);
            }

            var foundMatch = Responses.FirstOrDefault(resp => resp.IsMatch(request));

            HttpResponseMessage response = null;

            if (foundMatch != null)
            {
                response = new HttpResponseMessage(foundMatch.StatusCode)
                {
                    Content = new StringContent(
                            content: foundMatch.Content,
                            encoding: System.Text.Encoding.UTF8,
                            mediaType: "application/json")
                };
            }

            if ((int)foundMatch.StatusCode < 200 || (int)foundMatch.StatusCode > 299)
            {
                string responseMessage = foundMatch?.Content;
                ServiceErrorResponse error = null;
                try
                {
                    OrchestrationErrorResponse orchestrationError = JsonConvert.DeserializeObject<OrchestrationErrorResponse>(responseMessage);
                    ServiceErrorResponse innerError = new ServiceErrorResponse()
                    {
                        ErrorCode = orchestrationError.ErrorCode,
                        Message = orchestrationError.Message,
                        Target = string.Join(",", orchestrationError.Targets),
                        Source = "OrchestrationService"
                    };
                    error = new ServiceErrorResponse("123456789", "OrchestrationService", innerError);
                }
                catch
                {
                    throw TraceCore.TraceException(new EventTraceActivity(), new Microsoft.Commerce.Payments.Common.FailedOperationException($"Failed to deserialize error response from OrchestrationService"));
                }

                throw TraceCore.TraceException(new EventTraceActivity(), new ServiceErrorResponseException() { Error = error, Response = response, HandlingType = ExceptionHandlingPolicy.ByPass });
            }

            return await Task.FromResult(response);
        }
    }
}
