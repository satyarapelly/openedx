// <copyright file="IFeatureContextFeature.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PidlFactory.V7.FeatureContextProcess
{
    using System;
    using System.Collections.Generic;
    using Microsoft.Commerce.Payments.PidlFactory.V7.PIDLFeatureProcess;

    /// <summary>
    /// Interface for feature used in feature context process
    /// </summary>
    public interface IFeatureContextFeature
    {
        List<Action<FeatureContext>> GetActions(FeatureContext featureContext);
    }
}
