// <copyright file="IFeature.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PidlFactory.V7.PIDLFeatureProcess
{
    using System;
    using System.Collections.Generic;
    using Microsoft.Commerce.Payments.PidlModel.V7;

    /// <summary>
    /// Interface for PIDL feature 
    /// </summary>
    public interface IFeature
    {
        List<Action<List<PIDLResource>, FeatureContext>> GetActions(FeatureContext featureContext);
    }
}
