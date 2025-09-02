// <copyright file="PostProcessor.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PidlFactory.V7.PIDLFeatureProcess
{
    using System;
    using System.Collections.Generic;
    using Microsoft.Commerce.Payments.PidlModel.V7;

    /// <summary>
    /// Class to orchestrate the feature enablement
    /// </summary>
    public class PostProcessor
    {
        public static void Process(List<PIDLResource> resources, IFeatureFactory factory, FeatureContext featureContext)
        {
            Dictionary<string, IFeature> features = factory.GetFeatures(featureContext);
            foreach (IFeature feature in features.Values)
            {
                List<Action<List<PIDLResource>, FeatureContext>> actions = feature.GetActions(featureContext);

                foreach (Action<List<PIDLResource>, FeatureContext> action in actions)
                {
                    action(resources, featureContext);
                }
            }
        }
    }
}