// <copyright file="SecondScreenSessionData.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Model.SessionService
{
    using System.Collections.Generic;
    using Microsoft.Commerce.Payments.PidlFactory.V7;

    public class SecondScreenSessionData : SessionData
    {
        public SecondScreenSessionData(string country, string family, string type, string language, PIDLData pidlData)
        {
            this.Country = country;
            if (country == null)
            {
                this.Country = "us";
            }

            this.Family = family;
            if (family == null)
            {
                this.Family = "credit_card";
            }

            this.PaymentType = type;

            this.Language = language;
            if (language == null)
            {
                this.Language = "en-US";
            }

            if (pidlData == null)
            {
                this.PidlData = new PIDLData();
            }
            else
            {
                this.PidlData = pidlData;
            }
        }

        public string Country { get; set; }

        public string Family { get; set; }

        public string PaymentType { get; set; }

        public string Language { get; set; }

        public PIDLData PidlData { get; }

        public void AddToPidlData(Dictionary<string, object> newPidlData)
        {
            foreach (var item in newPidlData)
            {
                this.PidlData.Add(item.Key, item.Value);
            }
        }
    }
}
