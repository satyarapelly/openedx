using Microsoft.Commerce.Payments.Common.Tracing;
using System.Diagnostics.Tracing;

namespace Microsoft.Commerce.Payments.Common.Web
{
    public static class QosEventLevelExtensions
    {
        public static EventLevel ToEventLevel(this QosEventLevel level)
        {
            return (EventLevel)(int)level;
        }
    }
}