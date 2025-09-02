// <copyright file="VersionedHttpControllerDescriptor.cs" company="Microsoft">Copyright (c) Microsoft 2013. All rights reserved.</copyright>

using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Microsoft.Commerce.Payments.Common.Web
{
    /// <summary>
    /// Represents metadata for a versioned controller, including its type and associated filters.
    /// </summary>
    public class VersionedControllerDescriptor
    {
        /// <summary>
        /// Gets the controller name used in routing.
        /// </summary>
        public string ControllerName { get; }

        /// <summary>
        /// Gets the controller <see cref="Type"/>.
        /// </summary>
        public Type ControllerType { get; }

        /// <summary>
        /// Gets the filters associated with this controller.
        /// </summary>
        public IReadOnlyList<IFilterMetadata> Filters { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="VersionedControllerDescriptor"/> class.
        /// </summary>
        /// <param name="controllerName">The controller name.</param>
        /// <param name="controllerType">The controller type.</param>
        /// <param name="filters">The filters associated with this controller.</param>
        public VersionedControllerDescriptor(string controllerName, Type controllerType, IList<IFilterMetadata>? filters = null)
        {
            if (string.IsNullOrWhiteSpace(controllerName))
                throw new ArgumentException("Controller name cannot be null or empty.", nameof(controllerName));

            ControllerName = controllerName;
            ControllerType = controllerType ?? throw new ArgumentNullException(nameof(controllerType));
            Filters = filters is not null ? new List<IFilterMetadata>(filters) : Array.Empty<IFilterMetadata>();
        }
    }
}
