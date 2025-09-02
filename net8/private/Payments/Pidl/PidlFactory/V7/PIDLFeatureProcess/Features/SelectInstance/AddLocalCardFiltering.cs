// <copyright file="AddLocalCardFiltering.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PidlFactory.V7.PIDLFeatureProcess
{
    using System;
    using System.Collections.Generic;
    using Microsoft.Commerce.Payments.PidlModel.V7;
    using Microsoft.Commerce.Payments.PXCommon;

    /// <summary>
    /// Class representing the UpdateLocalDataSourceConfig, which updates data source for local card PIDLs with a configuration
    /// </summary>
    internal class AddLocalCardFiltering : IFeature
    {
        public List<Action<List<PIDLResource>, FeatureContext>> GetActions(FeatureContext featureContext)
        {
            return new List<Action<List<PIDLResource>, FeatureContext>>()
            {
                ModifyLocalDataSourceConfig
            };
        }

        internal static void ModifyLocalDataSourceConfig(List<PIDLResource> inputResources, FeatureContext featureContext)
        {
            if (string.Equals(featureContext.OperationType, V7.Constants.PidlOperationTypes.SelectInstance, StringComparison.OrdinalIgnoreCase))
            {
                List<string> allowedCountries = featureContext.SmdMarkets ?? new List<string>();

                if (!allowedCountries.Contains(featureContext.Country?.ToLower()))
                {
                    allowedCountries = new List<string> { featureContext.Country?.ToLower() };
                }

                foreach (PIDLResource pidlResource in inputResources)
                {
                    var dataSources = pidlResource.DataSources;

                    if (dataSources != null && dataSources[Constants.DataSourceConstants.PaymentInstruments] != null)
                    {
                        var piConfig = dataSources[Constants.DataSourceConstants.PaymentInstruments];

                        if (piConfig != null && piConfig.DataSourceConfig == null)
                        {
                            var functionContext = new Dictionary<string, List<string>>();
                            functionContext.Add(Constants.DataSourceConstants.AllowedCountries, allowedCountries);

                            piConfig.DataSourceConfig = new DataSourceConfig
                            {
                                UseLocalDataSource = true,
                                Filter = new DataSourceConfigFilters
                                {
                                    FunctionName = Constants.DataSourceConstants.FilterPaymentInstrumentsByCountry,
                                    FunctionContext = functionContext
                                }
                            };
                        }
                    }
                }
            }
        }
    }
}