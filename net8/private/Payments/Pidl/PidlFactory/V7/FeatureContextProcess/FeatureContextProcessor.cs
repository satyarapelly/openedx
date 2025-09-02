// <copyright file="FeatureContextProcessor.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PidlFactory.V7.FeatureContextProcess
{
    using System;
    using System.Collections.Generic;
    using Microsoft.Commerce.Payments.PidlFactory.V7.PIDLFeatureProcess;

    /// <summary>
    /// Class to orchestrate the feature enablement in feature context process
    /// </summary>
    public class FeatureContextProcessor
    {
        public static void Process(FeatureContext featureContext, IFeatureContextFactory factory)
        {
            Dictionary<string, IFeatureContextFeature> features = factory.GetFeatures(featureContext);
            foreach (IFeatureContextFeature feature in features.Values)
            {
                List<Action<FeatureContext>> actions = feature.GetActions(featureContext);

                foreach (Action<FeatureContext> action in actions)
                {
                    action(featureContext);
                }
            }
        }
    }
}