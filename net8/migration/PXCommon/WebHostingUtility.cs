// <copyright file="IsApplicationSelfHosted.cs" company="Microsoft Corporation">
using System;
using System.Diagnostics;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.Commerce.Payments.PXCommon
{
    /// <summary>
    /// Utilities to detect hosting model in ASP.NET Core / .NET 8.
    /// </summary>
    public static class WebHostingUtility
    {
        /// <summary>
        /// Returns true if the app is "self-hosted" (e.g., Kestrel/console/tests)
        /// and false if it is hosted by IIS (in-process or out-of-process).
        /// Prefer this overload when you can access DI.
        /// </summary>
        public static bool IsApplicationSelfHosted(IServiceProvider services)
        {
            if (services == null)
            {
                return IsApplicationSelfHosted(); // fall back
            }

            // If we're in-process under IIS, IServer type is IISHttpServer.
            var server = services.GetService<IServer>();
            var serverType = server?.GetType().FullName ?? string.Empty;
            if (serverType.IndexOf("IISHttpServer", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return false; // hosted by IIS (in-process)
            }

            // Heuristics for IIS out-of-process (IIS reverse proxy to Kestrel):
            if (IsIisEnvironmentVariablePresent())
            {
                return false;
            }

            // Last-resort process name check.
            var pname = TryGetProcessName();
            if (pname.Equals("w3wp", StringComparison.OrdinalIgnoreCase) ||
                pname.Equals("iisexpress", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            // Otherwise treat as self-hosted (Kestrel/console/tests/containers/etc.)
            return true;
        }

        /// <summary>
        /// Minimal heuristic (no DI): true if not under IIS; false if under IIS.
        /// </summary>
        public static bool IsApplicationSelfHosted()
        {
            if (IsIisEnvironmentVariablePresent())
            {
                return false;
            }

            var pname = TryGetProcessName();
            if (pname.Equals("w3wp", StringComparison.OrdinalIgnoreCase) ||
                pname.Equals("iisexpress", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            return true;
        }

        private static bool IsIisEnvironmentVariablePresent()
        {
            // Set by IIS (ANCM) in many configurations (in-process or out-of-process).
            // We check a few commonly-present variables to avoid false positives.
            return Environment.GetEnvironmentVariable("ASPNETCORE_IIS_PHYSICAL_PATH") != null
                || Environment.GetEnvironmentVariable("ASPNETCORE_IIS_HTTPAUTH") != null
                || Environment.GetEnvironmentVariable("IIS_WEBSITES_NAME") != null
                || Environment.GetEnvironmentVariable("WEBSITE_SITE_NAME") != null   // Azure App Service (IIS-based)
                || Environment.GetEnvironmentVariable("IIS_APP_POOL_ID") != null;    // in-process
        }

        private static string TryGetProcessName()
        {
            try
            {
                using var p = Process.GetCurrentProcess();
                return p.ProcessName;
            }
            catch
            {
                return string.Empty;
            }
        }
    }
}
