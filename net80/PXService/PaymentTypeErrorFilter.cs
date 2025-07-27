// <copyright file="PaymentTypeErrorFilter.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    public class PaymentTypeErrorFilter : ConfigurationObject
    {
        public PaymentTypeErrorFilter(List<string[]> rows, ParsedConfigurationComponent parsedComponent, Dictionary<string, int> columnNames)
        {
            this.TypesFilter = new HashSet<string>();
            this.CountryFilter = new HashSet<string>();
            this.ContainedIn_Partner = true;
            this.PartnerFilter = new HashSet<string>();
            this.ErrorDetails = ErrorDetailsFilter.ConstructFromConfiguration<ErrorDetailsFilter>(rows, parsedComponent, columnNames);

            for (int index = parsedComponent.Range.Item1; index <= parsedComponent.Range.Item2; index++)
            {
                string paymentMethodType = rows[index][(int)ResourceLifecycleStateManager.ErrorConfigColumn.PaymentMethodType];

                if (!string.IsNullOrEmpty(paymentMethodType))
                {
                    this.TypesFilter.Add(ResourceLifecycleStateManager.ParseConstant(paymentMethodType));
                }
                else
                {
                    break;
                }
            }

            for (int index = parsedComponent.Range.Item1; index <= parsedComponent.Range.Item2; index++)
            {
                string country = rows[index][(int)ResourceLifecycleStateManager.ErrorConfigColumn.Country];

                if (!string.IsNullOrEmpty(country))
                {
                    this.CountryFilter.Add(country);
                }
                else
                {
                    break;
                }
            }

            if (rows[parsedComponent.Range.Item1][(int)ResourceLifecycleStateManager.ErrorConfigColumn.ContainedIn_Partner] == "FALSE")
            {
                this.ContainedIn_Partner = false;
            }

            for (int index = parsedComponent.Range.Item1; index <= parsedComponent.Range.Item2; index++)
            {
                string partner = rows[index][(int)ResourceLifecycleStateManager.ErrorConfigColumn.Partner];

                if (!string.IsNullOrEmpty(partner))
                {
                    this.PartnerFilter.Add(ResourceLifecycleStateManager.ParseConstant(partner));
                }
                else
                {
                    break;
                }
            }
        }

        public ErrorDetailsFilter ErrorDetails { get; private set; }

        public bool ContainedIn_Partner { get; private set; }

        public HashSet<string> CountryFilter { get; private set; }

        public HashSet<string> PartnerFilter { get; private set; }

        public HashSet<string> TypesFilter { get; private set; }

        public bool MatchesState(ResourceLifecycleStateManager.ErrorResourceState state)
        {
            if (this.TypesFilter.Count > 0 && !this.TypesFilter.Contains(state.PaymentMethodType))
            {
                return false;
            }

            if (this.CountryFilter.Count > 0 && !this.CountryFilter.Contains(state.Country))
            {
                return false;
            }

            if (this.CountryFilter.Count > 0)
            {
                if (this.ContainedIn_Partner)
                {
                    if (!this.PartnerFilter.Contains(state.Partner))
                    {
                        return false;
                    }
                }
                else
                {
                    if (this.PartnerFilter.Contains(state.Partner))
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            if (this.TypesFilter.Count > 0)
            {
                sb.Append("Types[");
                sb.Append(this.TypesFilter.Aggregate((a, n) => a + "," + n));
                sb.Append(']');
            }

            if (this.CountryFilter.Count > 0)
            {
                if (sb.Length > 0)
                {
                    sb.Append(", ");
                }

                sb.Append("Countries[");
                sb.Append(this.CountryFilter.Aggregate((a, n) => a + "," + n));
                sb.Append(']');
            }

            if (this.PartnerFilter.Count > 0)
            {
                if (sb.Length > 0)
                {
                    sb.Append(", ");
                }

                if (!this.ContainedIn_Partner)
                {
                    sb.Append("not ");
                }

                sb.Append("in Partners[");
                sb.Append(this.PartnerFilter.Aggregate((a, n) => a + "," + n));
                sb.Append(']');
            }

            if (sb.Length == 0)
            {
                sb.Append("No filters");
            }

            return sb.ToString();
        }
    }
}