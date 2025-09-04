namespace Microsoft.Commerce.Payments.Common.Tracing
{
    using System.Diagnostics.Tracing;

    public static class EventLevelExtensions
    {
        public static QosEventLevel ToQosEventLevel(this EventLevel level)
        {
            return level switch
            {
                EventLevel.Error => QosEventLevel.Error,
                EventLevel.Warning => QosEventLevel.Warning,
                EventLevel.Informational => QosEventLevel.Information,
                _ => QosEventLevel.Trace,
            };
        }
    }
}
