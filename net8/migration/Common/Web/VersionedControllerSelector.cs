// <copyright file="VersionedControllerSelector.cs" company="Microsoft Corporation">Copyright (c) Microsoft 2013. All rights reserved.</copyright>

using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;

namespace Microsoft.Commerce.Payments.Common.Web
{
    public class VersionedControllerSelector
    {
        private readonly ILogger<VersionedControllerSelector> logger;
        private readonly Dictionary<string, Dictionary<string, Type>> versionedControllers = new(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, Type> versionlessControllers = new(StringComparer.OrdinalIgnoreCase);

        public VersionedControllerSelector(ILogger<VersionedControllerSelector> logger) => this.logger = logger;

        private static string Normalize(string name)
            => name.EndsWith("Controller", StringComparison.OrdinalIgnoreCase)
                ? name[..^"Controller".Length]
                : name;

        public void AddVersion(string version, Dictionary<string, Type> controllerMappings)
        {
            var normalized = new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase);
            foreach (var pair in controllerMappings)
            {
                normalized[Normalize(pair.Key)] = pair.Value;
            }

            if (!versionedControllers.TryAdd(version, normalized))
            {
                logger.LogWarning("Controller version {Version} already registered. Overwriting.", version);
                versionedControllers[version] = normalized;
            }
        }

        public void AddVersionless(string controllerName, Type controllerType)
            => versionlessControllers[Normalize(controllerName)] = controllerType;

        public Type? ResolveAllowedController(HttpContext context)
        {
            var routeValues = context.GetRouteData()?.Values;
            if (routeValues is null) return null;

            routeValues.TryGetValue("controller", out var controllerValue);
            var controllerName = Normalize(controllerValue?.ToString() ?? string.Empty);
            if (string.IsNullOrEmpty(controllerName)) return null;

            // Prefer header, else try path like /v7.0/...
            var version = context.Request.Headers["api-version"].ToString();
            if (string.IsNullOrWhiteSpace(version))
            {
                var path = context.Request.Path.Value ?? "";
                if (path.StartsWith("/v", StringComparison.OrdinalIgnoreCase))
                {
                    var seg = path.AsSpan(2).ToString().Split('/', 2)[0]; // "7.0"
                    version = "v" + seg;
                }
            }
            else if (!version.StartsWith("v", StringComparison.OrdinalIgnoreCase))
            {
                version = "v" + version;
            }

            logger.LogDebug("Resolving controller '{Controller}' for version '{Version}'", controllerName, version);

            if (!string.IsNullOrEmpty(version) && versionedControllers.TryGetValue(version, out var versionMap))
            {
                if (versionMap.TryGetValue(controllerName, out var controllerType))
                    return controllerType;
            }

            if (versionlessControllers.TryGetValue(controllerName, out var fallbackType))
                return fallbackType;

            logger.LogWarning("No controller found for '{Controller}' in version '{Version}'", controllerName, version);
            return null;
        }
    }
}
