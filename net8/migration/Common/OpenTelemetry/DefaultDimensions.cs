// <copyright file="DefaultDimensions.cs" company="Microsoft">Copyright (c) Microsoft 2025. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.Common.OpenTelemetry
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    /// <summary>
    /// Values for static dimensions.
    /// </summary>
    public static class DefaultDimensions
    {
        /// <summary>
        /// Name of the Azure Region, i.e. Central US.
        /// </summary>
        private static readonly string RegionNameValue = Environment.GetEnvironmentVariable("REGION_NAME") ?? string.Empty;

        /// <summary>
        /// Name of the AppService application, i.e. my-application-cus (without .azurewebsites.net).
        /// </summary>
        private static readonly string SiteNameValue = Environment.GetEnvironmentVariable("WEBSITE_SITE_NAME") ?? string.Empty;

        /// <summary>
        /// Name of the deployment slot, i.e. Production or Staging. It is set in ARM template.
        /// Defaulting it to string.Empty to avoid "Unsupported msgpack type: MSGPACK_OBJECT_NIL" in Geneva if it is not set yet.
        /// </summary>
        private static readonly string SlotNameValue = Environment.GetEnvironmentVariable("SlotName") ?? string.Empty;

        /// <summary>
        /// Name of the web job within the AppService application.
        /// </summary>
        private static readonly string WebJobNameValue = Environment.GetEnvironmentVariable("WEBJOBS_NAME") ?? string.Empty;

        /// <summary>
        /// Name of the machine.
        /// </summary>
        private static readonly string MachineNameValue = Environment.MachineName ?? string.Empty;

        /// <summary>
        /// The build version.
        /// </summary>
        private static readonly string BuildVersionValue = Environment.GetEnvironmentVariable("ServiceVersion") ?? string.Empty;

        /// <summary>
        /// Gets region name dimension.
        /// </summary>
        public static KeyValuePair<string, string> RegionName { get => new KeyValuePair<string, string>("RegionName", RegionNameValue); }

        /// <summary>
        /// Gets site name dimension.
        /// </summary>
        public static KeyValuePair<string, string> SiteName { get => new KeyValuePair<string, string>("SiteName", SiteNameValue); }

        /// <summary>
        /// Gets slot name dimension.
        /// </summary>
        public static KeyValuePair<string, string> SlotName { get => new KeyValuePair<string, string>("SlotName", SlotNameValue); }

        /// <summary>
        /// Gets machine name dimension.
        /// </summary>
        public static KeyValuePair<string, string> MachineName { get => new KeyValuePair<string, string>("MachineName", MachineNameValue); }

        /// <summary>
        /// Gets web job name dimension.
        /// </summary>
        public static KeyValuePair<string, string> WebJobName { get => new KeyValuePair<string, string>("WebJobName", WebJobNameValue); }

        /// <summary>
        /// Gets build version dimension.
        /// </summary>
        public static KeyValuePair<string, string> BuildVersion { get => new KeyValuePair<string, string>("BuildVersion", BuildVersionValue); }
    }
}
