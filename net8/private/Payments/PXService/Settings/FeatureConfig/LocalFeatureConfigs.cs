// <copyright file="LocalFeatureConfigs.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Settings.FeatureConfig
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.Tracing;
    using System.Linq;
    using Microsoft.Commerce.Payments.Common.Tracing;
    using Microsoft.Commerce.Payments.Common.Web;
    using Newtonsoft.Json;

    /// <summary>
    /// The object to store local feature configuration
    /// The config stores feature flags which need to be on through feature flag migration
    /// 1. hard to remove. eg. psd2 flags
    /// 2. need continue updating eg. trade avs to enlight more countries
    /// </summary>
    public class LocalFeatureConfigs
    {
        private const string AvailableToAllTag = "available_to_all";
        private const string PartnersTag = "partners";
        private const string CountriesTag = "countries";
        private List<string> allowedConditionTags = new List<string>()
        {
            AvailableToAllTag,
            PartnersTag,
            CountriesTag
        };

        /// <summary>
        /// Initializes a new instance of the <see cref="LocalFeatureConfigs"/> class. It is loading raw configuration as below to searchable structures 
        /// (more detail about each structure can be found in the member variables summary below)
        /// </summary>
        /// <param name="rawFeatures">
        /// {
        ///  "PXEnableAVSSuggestions": {
        ///    "countries": "am,az,br,by,hu,iq,kg,kz,md,mm,pl,ru,sa,ss,th,tj,ua,us,uz,vn,za"
        ///   },
        ///  "PXPSD2ProdIntegration": {
        ///    "partners": "cart,amcweb,xboxweb",
        ///    "allowed_test_user_group": true
        ///   },
        ///   "PXPSD2Comp-_-_-_-Succeeded": {
        ///   "available_to_all": true
        /// } 
        /// </param>
        /// <param name="testAccountsEnabledForAllFeatures">test account ids have enabled for all the features listed above</param>
        public LocalFeatureConfigs(
            Dictionary<string, Dictionary<string, string>> rawFeatures, 
            List<string> testAccountsEnabledForAllFeatures)
        {
            this.TestAccountIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            this.FeaturesEnabledForAll = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            this.FeaturesPerCountry = new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase);
            this.FeaturesPerPartner = new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase);
            this.LiveFeatures = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            if (testAccountsEnabledForAllFeatures != null && testAccountsEnabledForAllFeatures.Count > 0)
            {
                // all the test accounts will be enabled for all the flights
                testAccountsEnabledForAllFeatures
                    .Distinct()
                    .ToList()
                    .ForEach(accountId => this.TestAccountIds.Add(accountId));
            }

            if (rawFeatures == null || rawFeatures.Count == 0)
            {
                return;
            }

            // fill in FeaturesEnabledForAll, the features are available to all the users
            rawFeatures
                .Where(feature => 
                feature.Value != null 
                && feature.Value.ContainsKey(AvailableToAllTag) 
                && feature.Value[AvailableToAllTag]?.Trim() == "true")
                .Select(feature => feature.Key)
                .Distinct()
                .ToList()
                .ForEach(feature => this.FeaturesEnabledForAll.Add(feature));

            // fill in LiveFeatures, all the live features which can be added for all the test users
            rawFeatures.Keys.Distinct().ToList().ForEach(feature => this.LiveFeatures.Add(feature));

            // fill in FeaturesPerCountry and FeaturesPerPartner, the features per country and partner
            rawFeatures.AsEnumerable().ToList().ForEach(
                feature =>
                {
                    if (feature.Value?.Keys == null || feature.Value.Keys.Count == 0)
                    {
                        return;
                    }

                    if (feature.Value.Keys.Count > 1 || !this.allowedConditionTags.Contains(feature.Value.Keys.ToList()[0]))
                    {
                        throw new ArgumentException($"feature only allow single condition, it can be either {CountriesTag}, {PartnersTag} or {AvailableToAllTag}. please check the example");
                    }

                    string countries = null;
                    if (feature.Value != null && feature.Value.TryGetValue(CountriesTag, out countries))
                    {
                        countries?.Split(',')?.ToList()?.ForEach(
                                country =>
                                {
                                    country = country.Trim();
                                    if (!FeaturesPerCountry.ContainsKey(country))
                                    {
                                        FeaturesPerCountry.Add(country, new HashSet<string>());
                                    }

                                    FeaturesPerCountry[country].Add(feature.Key);
                                });
                    }

                    string partners = null;
                    if (feature.Value != null && feature.Value.TryGetValue(PartnersTag, out partners))
                    {
                        partners.Split(',').ToList().ForEach(
                                partner =>
                                {
                                    partner = partner.Trim();
                                    if (!FeaturesPerPartner.ContainsKey(partner))
                                    {
                                        FeaturesPerPartner.Add(partner, new HashSet<string>());
                                    }

                                    FeaturesPerPartner[partner].Add(feature.Key);
                                });
                    }
                });
        }

        /// <summary>
        /// Gets the dictionary which stores features sets enabled for each partner
        /// eg. xbox => ["feature1", "feature2", "feature3"]
        ///     amcweb => ["feature1", "feature2"]
        /// </summary>
        public Dictionary<string, HashSet<string>> FeaturesPerPartner { get; }

        /// <summary>
        /// Gets the dictionary which stores features sets enabled for each country
        /// eg. us => ["feature1", "feature2", "feature3"]
        ///     ca => ["feature1", "feature2"]
        /// </summary>
        public Dictionary<string, HashSet<string>> FeaturesPerCountry { get; }

        /// <summary>
        /// Gets the set which stores features enabled for all the traffic. The features serve 100% traffic. 
        /// The reason we can't remove since it is used as a configuration and code has the complexity to remove it
        /// </summary>
        public HashSet<string> FeaturesEnabledForAll { get; }

        /// <summary>
        /// Gets all the features are available/on.
        /// </summary>
        public HashSet<string> LiveFeatures { get; }

        /// <summary>
        /// Gets all test account ids will be available for all the flights
        /// </summary>
        public HashSet<string> TestAccountIds { get; }

        public static List<string> MergeMatchedEligibleLocalAndRemoteFeatures(
           string accountId,
           string partner,
           string country,
           LocalFeatureConfigs localFeatureConfigs,
           List<string> remoteFeatures,
           EventTraceActivity eventTraceActivity)
        {
            if (remoteFeatures == null)
            {
                remoteFeatures = new List<string>();
            }

            try
            {
                var localFeatures = localFeatureConfigs?.GetEligibleFeatures(accountId, partner, country);

                if (localFeatures == null || localFeatures.Count == 0)
                {
                    return remoteFeatures;
                }

                var featuresNotInLocal = remoteFeatures.Except(localFeatures)?.ToList();
                SllWebLogger.TraceServerMessage(
                   "LocalFeatureFlags",
                   eventTraceActivity.ToString(),
                   null,
                   $"remoteFeaturesNotInLocalCount: {featuresNotInLocal.Count}" +
                   $"remoteFeaturesNotInLocal: {JsonConvert.SerializeObject(featuresNotInLocal)}" +
                   $"LocalFeatures count isn't same as exposableFeatures local:{JsonConvert.SerializeObject(localFeatures)}",
                   EventLevel.Warning);

                remoteFeatures.AddRange(localFeatures);
            }
            catch (Exception ex)
            {
                SllWebLogger.TracePXServiceException(
                        ex.Message,
                        eventTraceActivity);
            }

            return remoteFeatures;
        }

        public List<string> GetEligibleFeatures(string accountId, string partner, string country)
        {
            if (this.TestAccountIds.Contains(accountId))
            {
                return this.LiveFeatures.ToList();
            }

            List<string> eligibleFeatures = new List<string>();
            HashSet<string> features = null;

            if (!string.IsNullOrEmpty(partner) && this.FeaturesPerPartner.TryGetValue(partner, out features))
            {
                eligibleFeatures.AddRange(features.ToList());
            }

            if (!string.IsNullOrEmpty(country) && this.FeaturesPerCountry.TryGetValue(country, out features))
            {
                eligibleFeatures.AddRange(features.ToList());
            }

            eligibleFeatures.AddRange(this.FeaturesEnabledForAll);

            return eligibleFeatures.Distinct().ToList();
        }
    }
}