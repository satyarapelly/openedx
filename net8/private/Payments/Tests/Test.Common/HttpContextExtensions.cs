using Microsoft.AspNetCore.Http;
using Microsoft.Commerce.Payments.Common.Transaction;

namespace Test.Common.Extensions
{
    public static class HttpContextExtensions
    {
        private static readonly string TestContextKey = "TestContext";

        public static bool TryGetTestContext(this HttpContext context, out TestContext? testContext)
        {
            if (context.Items.TryGetValue(TestContextKey, out var value) && value is TestContext ctx)
            {
                testContext = ctx;
                return true;
            }

            testContext = null;
            return false;
        }

        public static void SetTestContext(this HttpContext context, TestContext testContext)
        {
            context.Items[TestContextKey] = testContext;
        }
    }
}
