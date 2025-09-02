// <copyright file="IFeatureFactory.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PidlFactory.V7.PIDLFeatureProcess
{
    using System.Collections.Generic;

    /// <summary>
    /// Interface for FeatureFactory
    /// </summary>
    public interface IFeatureFactory
    {
        Dictionary<string, IFeature> GetFeatures(FeatureContext featureParams);
    }
}
