// <copyright file="ErrorDetails.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    public class ErrorDetails : ConfigurationObject
    {
        public ErrorDetails(List<string[]> rows, ParsedConfigurationComponent parsedComponent, Dictionary<string, int> columnNames)
        {
            this.Message = ResourceLifecycleStateManager.ParseConstant(rows[parsedComponent.Range.Item1][(int)ResourceLifecycleStateManager.ErrorConfigColumn.Message]);
            this.DetailsErrorCode = string.IsNullOrEmpty(rows[parsedComponent.Range.Item1][(int)ResourceLifecycleStateManager.ErrorConfigColumn.DetailsErrorCode]) ? null : ResourceLifecycleStateManager.ParseConstant(rows[parsedComponent.Range.Item1][(int)ResourceLifecycleStateManager.ErrorConfigColumn.DetailsErrorCode]);
            this.DetailsMessage = string.IsNullOrEmpty(rows[parsedComponent.Range.Item1][(int)ResourceLifecycleStateManager.ErrorConfigColumn.DetailsMessage]) ? null : ResourceLifecycleStateManager.ParseConstant(rows[parsedComponent.Range.Item1][(int)ResourceLifecycleStateManager.ErrorConfigColumn.DetailsMessage]);
            this.DetailsTarget = string.IsNullOrEmpty(rows[parsedComponent.Range.Item1][(int)ResourceLifecycleStateManager.ErrorConfigColumn.DetailsTarget]) ? null :
                rows.Skip(parsedComponent.Range.Item1).Take(parsedComponent.Range.Item2 - parsedComponent.Range.Item1 + 1)
                     .TakeWhile(row => !string.IsNullOrEmpty(row[(int)ResourceLifecycleStateManager.ErrorConfigColumn.DetailsTarget]))
                     .Select(row => ResourceLifecycleStateManager.ParseConstant(row[(int)ResourceLifecycleStateManager.ErrorConfigColumn.DetailsTarget]))
                     .Aggregate((a, n) => a + "," + n);
            this.ClientAction = string.IsNullOrEmpty(rows[parsedComponent.Range.Item1][(int)ResourceLifecycleStateManager.ErrorConfigColumn.ClientAction]) ?
                ResourceLifecycleStateManager.ErrorClientAction.None :
                (ResourceLifecycleStateManager.ErrorClientAction)Enum.Parse(typeof(ResourceLifecycleStateManager.ErrorClientAction), rows[parsedComponent.Range.Item1][(int)ResourceLifecycleStateManager.ErrorConfigColumn.ClientAction]);
        }

        public ResourceLifecycleStateManager.ErrorClientAction ClientAction { get; private set; }

        public string DetailsErrorCode { get; private set; }

        public string DetailsMessage { get; private set; }

        public string DetailsTarget { get; private set; }

        public string Message { get; private set; }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            if (string.IsNullOrEmpty(this.DetailsErrorCode) && string.IsNullOrEmpty(this.DetailsMessage) && string.IsNullOrEmpty(this.DetailsTarget))
            {
                sb.Append("Message: ");
                sb.Append(this.Message);
            }
            else
            {
                if (!string.IsNullOrEmpty(this.DetailsErrorCode))
                {
                    sb.Append("DetailsErrorCode: ");
                    sb.Append(this.DetailsErrorCode);
                }

                if (!string.IsNullOrEmpty(this.DetailsMessage))
                {
                    if (sb.Length > 0)
                    {
                        sb.Append(", ");
                    }

                    sb.Append("DetailsMessage: ");
                    sb.Append(this.DetailsMessage);
                }

                if (!string.IsNullOrEmpty(this.DetailsTarget))
                {
                    if (sb.Length > 0)
                    {
                        sb.Append(", ");
                    }

                    sb.Append("DetailsTarget: ");
                    sb.Append(this.DetailsTarget);
                }
            }

            return sb.ToString();
        }
    }
}