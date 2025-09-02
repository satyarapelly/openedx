// <copyright file="IFeatureContextFactory.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PidlFactory.V7.FeatureContextProcess
{
    using System.Collections.Generic;
    using Microsoft.Commerce.Payments.PidlFactory.V7.PIDLFeatureProcess;

    /// <summary>
    /// Interface for FeatureContextFactory
    /// </summary>
    public interface IFeatureContextFactory
    {
        Dictionary<string, IFeatureContextFeature> GetFeatures(FeatureContext featureParams);
    }
}
