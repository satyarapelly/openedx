namespace Microsoft.Commerce.Payments.PXCommon
{
    using Microsoft.Commerce.Tracing;

    /// <summary>
    /// Provides minimal logging helpers used by the web8 project.
    /// </summary>
    public static class SllWebLogger
    {
        public static void TracePXServiceException(string message, EventTraceActivity traceActivityId)
        {
            // In the original implementation this would log to Microsoft internal
            // tracing infrastructure. For the simplified .NET 8 port we simply
            // write to the console so callers have a basic diagnostic trail.
            System.Console.WriteLine($"[PXService] {traceActivityId} {message}");
        }
    }
}
