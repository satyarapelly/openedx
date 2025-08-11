// <copyright file="VersionedHttpControllerDescriptor.cs" company="Microsoft">Copyright (c) Microsoft 2013. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.Common.Web
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Web.Http;
    using System.Web.Http.Controllers;
    using System.Web.Http.Filters;

    internal class VersionedHttpControllerDescriptor : HttpControllerDescriptor
    {
        private Collection<IFilter> filters;

        public VersionedHttpControllerDescriptor(HttpConfiguration configuration, string controllerName, Type controllerType, List<IFilter> filters)
            : base(configuration, controllerName, controllerType)
        {
            this.filters = new Collection<IFilter>(filters);
        }

        public override Collection<IFilter> GetFilters()
        {
            Collection<IFilter> existingFilters = base.GetFilters();
            return new Collection<IFilter>(existingFilters.Concat(this.filters).ToList());
        }
    }
}
