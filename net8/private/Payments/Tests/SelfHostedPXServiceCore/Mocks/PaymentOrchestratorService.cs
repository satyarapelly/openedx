namespace SelfHostedPXServiceCore.Mocks
{
    using Microsoft.Commerce.Payments.Tests.Emulators.PXDependencyEmulators.Mocks;
    using Test.Common;

    public class PaymentOrchestratorService : MockServiceWebRequestHandler
    {
        public readonly PaymentOrchestratorServiceMockResponseProvider ResponseProvider;

        public PaymentOrchestratorService(PaymentOrchestratorServiceMockResponseProvider responseProvider, bool useArrangedResponses) : base(responseProvider, useArrangedResponses)
        {
            ResponseProvider = responseProvider;
        }
    }
}
