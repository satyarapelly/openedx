// <copyright file="MoveSelectedPIToFirstOption.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PidlFactory.V7.PIDLFeatureProcess
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.Commerce.Payments.PidlModel.V7;

    /// <summary>
    /// Class representing the MoveSelectedPIToFirstOption, which moves the selectedPI to first option making it default instance 
    /// and Id is passed in filter.Id for selecting PI.
    /// </summary>
    internal class MoveSelectedPIToFirstOption : IFeature
    {
        public List<Action<List<PIDLResource>, FeatureContext>> GetActions(FeatureContext featureContext)
        {
            return new List<Action<List<PIDLResource>, FeatureContext>>()
            {
                MoveSelectedPIToFirstOptionForSelectInstance
            };
        }

        internal static void MoveSelectedPIToFirstOptionForSelectInstance(List<PIDLResource> inputResources, FeatureContext featureContext)
        {
            if (string.Equals(featureContext.OperationType, V7.Constants.PidlOperationTypes.SelectInstance, StringComparison.OrdinalIgnoreCase))
            {
                foreach (PIDLResource pidlResource in inputResources)
                {                    
                    PropertyDisplayHint paymentInstrument = pidlResource.GetDisplayHintById(Constants.DisplayHintIds.PaymentInstrumentSelect) as PropertyDisplayHint;
                    
                    if (paymentInstrument != null && PaymentSelectionHelper.TryGetPaymentMethodFilters(featureContext.Filters, out PaymentMethodFilters filters)
                        && !string.IsNullOrEmpty(filters?.Id))
                    {
                        string defaultInstanceId = filters.Id;
                        Dictionary<string, string> possibleValues = paymentInstrument.PossibleValues;
                        Dictionary<string, SelectOptionDescription> possibleOptions = paymentInstrument.PossibleOptions;

                        // Move the selected PI to first in possibleValues
                        MoveItemToFirstInDictionary<string>(defaultInstanceId, ref possibleValues);
                        paymentInstrument.PossibleValues = possibleValues;

                        // Move the selected PI to first in possibleOptions
                        MoveItemToFirstInDictionary<SelectOptionDescription>(defaultInstanceId, ref possibleOptions);
                        paymentInstrument.PossibleOptions = possibleOptions;

                        if ((possibleValues != null && possibleValues.ContainsKey(defaultInstanceId))
                            || (possibleOptions != null && possibleOptions.ContainsKey(defaultInstanceId)))
                        {
                            paymentInstrument.IsSelectFirstItem = true;
                        }

                        // Update the data description possibleValues & defaultValue
                        if (pidlResource.DataDescription != null && pidlResource.DataDescription.ContainsKey(Constants.DataDescriptionIds.Id))
                        {
                            var propertyHint = pidlResource.DataDescription[Constants.DataDescriptionIds.Id] as PropertyDescription;
                            propertyHint.UpdatePossibleValues(paymentInstrument.PossibleValues);

                            if (string.Equals(paymentInstrument.SelectType, Constants.PaymentMethodSelectType.DropDown, StringComparison.InvariantCultureIgnoreCase)
                                || string.Equals(paymentInstrument.SelectType, Constants.PaymentMethodSelectType.Radio, StringComparison.InvariantCultureIgnoreCase))
                            {
                                if (propertyHint.PossibleValues != null && propertyHint.PossibleValues.Count != 0)
                                {
                                    propertyHint.DefaultValue = (!string.IsNullOrEmpty(defaultInstanceId) && propertyHint.PossibleValues.ContainsKey(defaultInstanceId)) ?
                                        defaultInstanceId : propertyHint.PossibleValues.First().Key;
                                }
                            }
                        }
                    }
                }
            }
        }

        private static void MoveItemToFirstInDictionary<T>(string itemId, ref Dictionary<string, T> possibleOptionsOrValues)
        {
            if (possibleOptionsOrValues != null && possibleOptionsOrValues.ContainsKey(itemId))
            {
                Dictionary<string, T> temp = new Dictionary<string, T>
                {
                    [itemId] = possibleOptionsOrValues[itemId]
                };

                foreach (string key in possibleOptionsOrValues.Keys.Where(key => !key.Equals(itemId, StringComparison.OrdinalIgnoreCase)))
                {
                    temp[key] = possibleOptionsOrValues[key];
                }

                possibleOptionsOrValues = temp;
            }
        }
    }
}