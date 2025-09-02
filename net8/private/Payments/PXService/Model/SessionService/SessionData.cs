// <copyright file="SessionData.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Model.SessionService
{
    using System.Collections.Generic;

    public class SessionData
    {
        public SessionData()
        {
            this.ConfigInfo = new Dictionary<string, string>();
        }

        public Dictionary<string, string> ConfigInfo { get; }

        public void AddToConfig(string key, string value)
        {
            this.ConfigInfo.Add(key, value);
        }

        public string GetFromConfig(string key)
        {
            return this.ConfigInfo[key];
        }
    }
}