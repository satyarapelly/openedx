namespace Tests.Common.Model.PurchaseService
{
    using System.Collections.Generic;

    public class PaymentInstrumentCheckResponse
    {
        public List<string>? OrderIds { get; set; }
        public bool PaymentInstrumentInUse { get; set; }
        public List<string>? RecurrenceIds { get; set; }
    }
}

namespace Tests.Common.Model.Pims
{
    using System.Collections.Generic;

    public class PaymentMethod
    {
        public string? PaymentMethodFamily { get; set; }
        public string? PaymentMethodType { get; set; }
    }

    public class PaymentInstrument
    {
        public string? PaymentInstrumentAccountId { get; set; }
        public string? PaymentInstrumentId { get; set; }
        public PaymentInstrumentDetails PaymentInstrumentDetails { get; set; } = new PaymentInstrumentDetails();
    }

    public class PaymentInstrumentDetails
    {
        public List<string> RequiredChallenge { get; set; } = new List<string>();
    }
}

namespace Test.Common
{
    using System.Collections.Generic;

    public class TestScenario
    {
        public Dictionary<string, TestResponse> ResponsesPerApiCall { get; set; } = new Dictionary<string, TestResponse>();
    }

    public class TestResponse
    {
        public object? Content { get; set; }
    }
}
