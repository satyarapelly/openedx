// <copyright file="VersionedControllerSelector.cs" company="Microsoft Corporation">Copyright (c) Microsoft 2013. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.Common.Web
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.Net.Http;
    using System.Web.Http;
    using System.Web.Http.Controllers;
    using System.Web.Http.Dispatcher;
    using System.Web.Http.Filters;
    using System.Web.Http.Routing;

    /// <summary>
    /// Implements a controller selector that takes api-version into account.
    /// </summary>
    public class VersionedControllerSelector : IHttpControllerSelector
    {
        private const string ControllerProperty = "controller";

        // NOTE: We use this as temp storage before Initialize is called.  When this
        // is non-null it indicates that we have not been initialized.
        private Dictionary<ApiVersion, Tuple<Dictionary<string, Type>, List<IFilter>>> tempControllerVersions;
        private Dictionary<string, Type> tempVersionlessControllers;

        private Dictionary<Version, Dictionary<string, HttpControllerDescriptor>> versionedDescriptors;
        private Dictionary<string, HttpControllerDescriptor> versionlessDescriptors;

        private ReadOnlyDictionary<string, ApiVersion> supportedVersions;

        /// <summary>
        /// Initializes a new instance of the <see cref="VersionedControllerSelector"/> class.
        /// </summary>
        public VersionedControllerSelector()
        {
            this.tempControllerVersions = new Dictionary<ApiVersion, Tuple<Dictionary<string, Type>, List<IFilter>>>();
            this.tempVersionlessControllers = new Dictionary<string, Type>();
        }

        /// <summary>
        /// Gets the dictionary of api-version to internal version for all supported versions.
        /// </summary>
        public IDictionary<string, ApiVersion> SupportedVersions
        {
            get
            {
                this.ThrowIfNotInitialized();
                return this.supportedVersions;
            }
        }

        public void Add(ApiVersion version, Dictionary<string, Type> controllers)
        {
            this.Add(version, controllers, new List<IFilter>());
        }

        public void AddVersionless(string controllerName, Type controllerType)
        {
            Debug.Assert(controllerName != null, "controllerName cannot be null");
            Debug.Assert(controllerType != null, "controllerType cannot be null");
            this.tempVersionlessControllers.Add(controllerName, controllerType);
        }

        /// <summary>
        /// Add a new supported version to the controller selector.
        /// </summary>
        /// <param name="version">The api version definition.</param>
        /// <param name="controllers">The dictionary from controller name (controller parameter on the route) to the controller type.</param>
        /// <param name="filters"> list of filters to be used with the version</param>
        public void Add(ApiVersion version, Dictionary<string, Type> controllers, List<IFilter> filters)
        {
            this.ThrowIfInitialized();

            Debug.Assert(version != null, "Version cannot be null");
            Debug.Assert(controllers != null, "Controllers cannot be null");
            Debug.Assert(controllers.Count > 0, "At least one controller type must be specified");

            this.tempControllerVersions.Add(version, new Tuple<Dictionary<string, Type>, List<IFilter>>(controllers, filters));
        }

        /// <summary>
        /// Initialize the VersionedControllerSelector.  This builds the internal data structures
        /// necessary to select the correct controller at runtime.
        /// </summary>
        /// <param name="sharedConfiguration">The HTTP configuration to be shared across all of
        /// the controllers.</param>
        /// <param name="initVersionHandler">whether initial the handler</param>
        public void Initialize(HttpConfiguration sharedConfiguration, bool initVersionHandler = true)
        {
            this.ThrowIfInitialized();

            Debug.Assert(sharedConfiguration != null, "The http configuration is null");
            Debug.Assert(this.tempControllerVersions.Count > 0, "Cannot initialize a VersionedControllerSelector which has not had any supported versions added through the Add method.");

            Dictionary<string, ApiVersion> tempSupportedVersions = new Dictionary<string, ApiVersion>(this.tempControllerVersions.Count);
            this.versionedDescriptors = new Dictionary<Version, Dictionary<string, HttpControllerDescriptor>>(this.tempControllerVersions.Count);

            foreach (KeyValuePair<ApiVersion, Tuple<Dictionary<string, Type>, List<IFilter>>> controllerVersion in this.tempControllerVersions)
            {
                // Add to the supported versions dictionary
                tempSupportedVersions.Add(controllerVersion.Key.ExternalVersion, controllerVersion.Key);

                // We're doing two important things in this loop:
                // * Switching from controller types to controller descriptors. It was recommended that we cache
                //   controller descriptors to avoid unnecessary processing.
                // * Making sure the dictionary uses an OrdinalIgnoreCase comparer to avoid casing mismatch issues.
                Dictionary<string, HttpControllerDescriptor> descriptors = new Dictionary<string, HttpControllerDescriptor>(controllerVersion.Value.Item1.Count, StringComparer.OrdinalIgnoreCase);
                foreach (KeyValuePair<string, Type> controller in controllerVersion.Value.Item1)
                {
                    VersionedHttpControllerDescriptor descriptor = new VersionedHttpControllerDescriptor(sharedConfiguration, controller.Key, controller.Value, controllerVersion.Value.Item2);
                    descriptors.Add(controller.Key, descriptor);
                }

                this.versionedDescriptors.Add(controllerVersion.Key.InternalVersion, descriptors);
            }

            this.supportedVersions = new ReadOnlyDictionary<string, ApiVersion>(tempSupportedVersions);

            this.versionlessDescriptors = new Dictionary<string, HttpControllerDescriptor>(this.tempVersionlessControllers.Count);
            foreach (KeyValuePair<string, Type> versionlessController in this.tempVersionlessControllers)
            {
                HttpControllerDescriptor descriptor = new HttpControllerDescriptor(sharedConfiguration, versionlessController.Key, versionlessController.Value);
                this.versionlessDescriptors.Add(versionlessController.Key, descriptor);
            }

            // We use our own controller selector so that
            // we can route based on api-version.
            sharedConfiguration.Services.Replace(typeof(IHttpControllerSelector), this);

            if (initVersionHandler)
            {
                // hook up the api version handler
                sharedConfiguration.MessageHandlers.Add(new ApiVersionHandler(this.supportedVersions));
            }

            this.MarkInitialized();
        }

        /// <summary>
        /// Gets a static mapping of controller names to controller descriptors.
        /// Returns null since the Versioned controller will may have multiple
        /// controllers for the same name leading to misleading Help pages.
        /// </summary>
        /// <returns>Always returns null.</returns>
        public IDictionary<string, HttpControllerDescriptor> GetControllerMapping()
        {
            return null;
        }

        /// <summary>
        /// Selects the appropriate controller based on api-version information
        /// extracted from the request.
        /// </summary>
        /// <param name="request">The HTTP request.</param>
        /// <returns>A descriptor for the controller to use for the provided
        /// message.</returns>
        public virtual HttpControllerDescriptor SelectController(HttpRequestMessage request)
        {
            this.ThrowIfNotInitialized();

            // The version was stamped by the ApiVersionHandler.
            ApiVersion apiVersion = null;
            object versionObject;
            if (request.Properties.TryGetValue(PaymentConstants.Web.Properties.Version, out versionObject))
            {
                apiVersion = (ApiVersion)versionObject;
            }

            string controllerName = string.Empty;

            // Get the controller name from the routing information.
            IHttpRouteData routeData = request.GetRouteData();
            object controllerProperty;
            if (routeData.Values.TryGetValue(ControllerProperty, out controllerProperty))
            {
                controllerName = controllerProperty as string;
            }

            if (apiVersion != null)
            {
                Debug.Assert(this.versionedDescriptors.ContainsKey(apiVersion.InternalVersion), "The ApiVersionHandler should have thrown if the version wasn't supported.");
                Dictionary<string, HttpControllerDescriptor> descriptors = this.versionedDescriptors[apiVersion.InternalVersion];

                HttpControllerDescriptor result;
                if (descriptors.TryGetValue(controllerName, out result))
                {
                    return result;
                }
            }
            else
            {
                HttpControllerDescriptor result;
                if (this.versionlessDescriptors.TryGetValue(controllerName, out result))
                {
                    return result;
                }
            }

            // Returning null here results in a 404.  This is the behavior we want
            // when the requested controller (IE - user provided URL) references a
            // controller that doesn't exist.
            return null;
        }

        /// <summary>
        /// Mark the selector as being initialized.  Separated out into a separate method
        /// so that it is easy to maintain this and the two Throw*Initialized methods.
        /// </summary>
        private void MarkInitialized()
        {
            this.tempControllerVersions = null;
            this.tempVersionlessControllers = null;
        }

        /// <summary>
        /// Throw an InvalidOperation is we're already initialized.
        /// </summary>
        private void ThrowIfInitialized()
        {
            if (this.tempControllerVersions == null)
            {
                throw new InvalidOperationException("The selector is already initialized.");
            }
        }

        /// <summary>
        /// Throw an InvalidOperation is we're not already initialized.
        /// </summary>
        private void ThrowIfNotInitialized()
        {
            if (this.tempControllerVersions != null)
            {
                throw new InvalidOperationException("The selector has not been initialized.");
            }
        }
    }
}
