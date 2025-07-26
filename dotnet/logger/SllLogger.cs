namespace Microsoft.Commerce.Payments.Common.Tracing
{
    using System.Diagnostics.Tracing;

    /// <summary>
    /// Minimal stand-in for the original SllLogger infrastructure.
    /// In this simplified repository it just writes log messages to the console.
    /// </summary>
    public static class SllLogger
    {
        /// <summary>
        /// Emits a trace message with the given level. The original
        /// implementation logged to SLL. Here we simply write to the console.
        /// </summary>
        public static void TraceMessage(string message, EventLevel level)
        {
            System.Console.WriteLine($"[{level}] {message}");
        }

        /// <summary>
        /// Enables realtime logging. The real SLL logger toggles additional
        /// diagnostic output. For our purposes it is a no-op apart from a
        /// console notification.
        /// </summary>
        public static void SetRealtimeLogging()
        {
            System.Console.WriteLine("Realtime logging enabled");
        }
    }
}
