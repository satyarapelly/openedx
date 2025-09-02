// <copyright file="ServiceErrorRules.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService
{
    using System;
    using System.Collections.Generic;

    public class ServiceErrorRules : ConfigurationObject
    {
        public ServiceErrorRules(List<string[]> rows, ParsedConfigurationComponent parsedComponent, Dictionary<string, int> columnNames)
        {
            this.Rules = new Dictionary<ResourceLifecycleStateManager.ErrorResourceAction, Dictionary<string, List<PaymentTypeErrorFilter>>>();

            foreach (ParsedConfigurationComponent actionComponent in parsedComponent["RequestType"])
            {
                ResourceLifecycleStateManager.ErrorResourceAction action = (ResourceLifecycleStateManager.ErrorResourceAction)Enum.Parse(typeof(ResourceLifecycleStateManager.ErrorResourceAction), rows[actionComponent.Range.Item1][(int)ResourceLifecycleStateManager.ErrorConfigColumn.ResourceAction]);

                this.Rules[action] = new Dictionary<string, List<PaymentTypeErrorFilter>>();

                foreach (ParsedConfigurationComponent serviceErrorRuleComponent in actionComponent["ServiceErrorRule"])
                {
                    string paymentMethodFamily = rows[serviceErrorRuleComponent.Range.Item1][(int)ResourceLifecycleStateManager.ErrorConfigColumn.PaymentMethodFamily];

                    if (!this.Rules[action].ContainsKey(paymentMethodFamily))
                    {
                        this.Rules[action][paymentMethodFamily] = new List<PaymentTypeErrorFilter>();
                    }

                    this.Rules[action][paymentMethodFamily].Add(PaymentTypeErrorFilter.ConstructFromConfiguration<PaymentTypeErrorFilter>(rows, serviceErrorRuleComponent, columnNames));
                }
            }
        }

        public Dictionary<ResourceLifecycleStateManager.ErrorResourceAction, Dictionary<string, List<PaymentTypeErrorFilter>>> Rules { get; private set; }

        public PaymentTypeErrorFilter GetMatchingPaymentTypeErrorFilter(ResourceLifecycleStateManager.ErrorResourceAction action, ResourceLifecycleStateManager.ErrorResourceState state)
        {
            PaymentTypeErrorFilter matchingFilter = null;

            if (this.Rules.ContainsKey(action))
            {
                if (this.Rules[action].ContainsKey(state.PaymentMethodFamily))
                {
                    foreach (PaymentTypeErrorFilter filter in this.Rules[action][state.PaymentMethodFamily])
                    {
                        if (filter.MatchesState(state))
                        {
                            matchingFilter = filter;
                            break;
                        }
                    }
                }
            }

            return matchingFilter;
        }
    }
}