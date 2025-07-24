namespace Microsoft.Commerce.Payments.PXCommon
{
    using System;
    using System.Diagnostics;

    /// <summary>
    /// Utility methods related to web hosting.
    /// </summary>
    public static class WebHostingUtility
    {
        /// <summary>
        /// Determines if the application is running without IIS (self hosted).
        /// </summary>
        /// <returns>
        /// <c>true</c> if the current process does not appear to be hosted by IIS; otherwise <c>false</c>.
        /// </returns>
        public static bool IsApplicationSelfHosted()
        {
            // .NET 8 does not provide System.Web.Hosting.HostingEnvironment. To
            // approximate the original check, detect if the current process is the
            // IIS worker process or IIS Express. If not, assume the application is
            // running using Kestrel or another self-hosted server.
            string processName = Process.GetCurrentProcess().ProcessName;
            return !processName.Equals("w3wp", StringComparison.OrdinalIgnoreCase)
                && !processName.Equals("iisexpress", StringComparison.OrdinalIgnoreCase);
        }
    }
}
