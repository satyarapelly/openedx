// <copyright file="ApiVersion.cs" company="Microsoft Corporation">Copyright (c) Microsoft 2013. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.Common.Web
{
    using System;
    using System.Diagnostics;

    /// <summary>
    /// Defines an external version (YYYY-MM-DD) to internal version (Major.Minor.Build.Revision) pair.
    /// </summary>
    public class ApiVersion
    {       
        /// <summary>
        /// Initializes a new instance of the <see cref="ApiVersion"/> class.
        /// </summary>
        /// <param name="apiVersion">The external version.  This is a string in YYYY-MM-DD format.</param>
        /// <param name="internalVersion">The internal version.</param>
        public ApiVersion(string apiVersion, Version internalVersion)
        {
            Debug.Assert(!string.IsNullOrWhiteSpace(apiVersion), "External api version is not set or empty");
            Debug.Assert(internalVersion != null, "Internal version not set");

            this.ExternalVersion = apiVersion;
            this.InternalVersion = internalVersion;
        }

        /// <summary>
        /// Gets the external version.  This is a string in YYYY-MM-DD format which the user passes
        /// using the api-version header or query parameter.
        /// </summary>
        public string ExternalVersion
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the internal version.
        /// </summary>
        public Version InternalVersion
        {
            get;
            private set;
        }
    }
}