// Copyright (c) Microsoft Corporation.

using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using Microsoft.Commerce.Payments.Common;

namespace Microsoft.Commerce.Payments.Common.Web
{
    /// <summary>
    /// Simplified controller selector for ASP.NET Core that routes requests
    /// to versioned controllers based on the resolved <see cref="ApiVersion"/>.
    /// </summary>
    public class VersionedControllerSelector
    {
        private readonly ILogger<VersionedControllerSelector> _logger;

        // Map of external version string (e.g. "v7.0") to controller mappings.
        private readonly Dictionary<string, Dictionary<string, Type>> _versionedControllers = new(StringComparer.OrdinalIgnoreCase);

        // Controllers that do not require a version (probe etc.).
        private readonly Dictionary<string, Type> _versionlessControllers = new(StringComparer.OrdinalIgnoreCase);

        // Supported API versions exposed to callers (external -> ApiVersion).
        private readonly Dictionary<string, ApiVersion> _supportedVersions = new(StringComparer.OrdinalIgnoreCase);

        public VersionedControllerSelector(ILogger<VersionedControllerSelector> logger) => _logger = logger;

        /// <summary>
        /// Gets the set of supported API versions.
        /// </summary>
        public IDictionary<string, ApiVersion> SupportedVersions => _supportedVersions;

        /// <summary>
        /// Registers controllers that are bound to a specific API version.
        /// </summary>
        /// <param name="version">External version string (for example "v7.0").</param>
        /// <param name="controllers">Controller name to type mappings.</param>
        public void AddVersion(string version, Dictionary<string, Type> controllers)
        {
            if (string.IsNullOrWhiteSpace(version))
            {
                throw new ArgumentNullException(nameof(version));
            }

            if (controllers is null || controllers.Count == 0)
            {
                throw new ArgumentException("At least one controller type must be specified", nameof(controllers));
            }

            if (!Version.TryParse(version.TrimStart('v', 'V'), out var internalVersion))
            {
                internalVersion = new Version(1, 0);
            }

            _supportedVersions[version] = new ApiVersion(version, internalVersion);
            _versionedControllers[version] = controllers;
        }

        /// <summary>
        /// Registers a controller that can be invoked without specifying an API version.
        /// </summary>
        public void AddVersionless(string controllerName, Type controllerType)
        {
            if (controllerName is null) throw new ArgumentNullException(nameof(controllerName));
            if (controllerType is null) throw new ArgumentNullException(nameof(controllerType));

            _versionlessControllers[controllerName] = controllerType;
        }

        /// <summary>
        /// Resolves the controller type allowed for the current request.
        /// Returns <c>null</c> when no matching controller exists.
        /// </summary>
        public Type? ResolveAllowedController(HttpContext context)
        {
            var routeValues = context.GetRouteData()?.Values;
            if (routeValues is null) return null;

            routeValues.TryGetValue("controller", out var controllerValue);
            var controllerName = controllerValue?.ToString();
            if (string.IsNullOrEmpty(controllerName)) return null;

            if (context.Items.TryGetValue(PaymentConstants.Web.Properties.Version, out var versionObj)
                && versionObj is ApiVersion apiVersion
                && _versionedControllers.TryGetValue(apiVersion.ExternalVersion, out var versionMap)
                && versionMap.TryGetValue(controllerName, out var controllerType))
            {
                return controllerType;
            }

            if (_versionlessControllers.TryGetValue(controllerName, out var versionlessType))
            {
                return versionlessType;
            }

            _logger.LogDebug("No controller found for '{Controller}'", controllerName);
            return null;
        }
    }
}

