// <copyright file="PXServiceRequestBuilder.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Spec.PXService.Steps
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Linq;

    public class PXServiceRequestBuilder
    {
        public string AccountId { get; set; }

        private readonly Uri baseUri;

        private bool isShippingV3Enabled;

        public bool IsShippingV3Enabled
        {
            get
            {
                return this.isShippingV3Enabled;
            }

            set
            {
                if (value)
                {
                    this.AddFlight("PXUseShippingV3ForCompletePrerequisites");
                }
                else
                {
                    if (this.flights.Contains("PXUseShippingV3ForCompletePrerequisites"))
                    {
                        this.flights.Remove("PXUseShippingV3ForCompletePrerequisites");
                    }
                }
                
                this.isShippingV3Enabled = value;
            }
        }

        private List<string> flights = new List<string>();

        private Dictionary<string, string> requestHeaders = new Dictionary<string, string>();

        public void AddHeader(string key, string value)
        {
            this.requestHeaders.Add(key, value);
        }

        public string MSAEmail { get; private set; }

        public string MSAPuid { get; private set; }

        public void SetMSAProfile(string email, string puid)
        {
            MSAEmail = email;
            MSAPuid = puid;

            AddHeader("x-ms-clientcontext-encoding", "base64");
            AddHeader("x-ms-msaprofile", string.Format("PUID={0},emailAddress={1}", puid.ToBase64(), email.ToBase64()));
        }

        public IDictionary<string, string> GetHeaders()
        {
            return this.requestHeaders;
        }

        public void AddFlight(string name)
        {
            this.flights.Add(name);
        }

        public IEnumerable<string> GetFlights()
        {
            return this.flights;
        }

        public string Filters
        {
            get => GetUrlParameter("filters");
            set => this.QueryString.Add("filters", value);
        }

        public string Type
        {
            get => GetUrlParameter("type");
            set => this.QueryString.Add("type", value);
        }

        public string Family
        {
            get => GetUrlParameter("family");
            set => this.QueryString.Add("family", value);
        }

        public string OrderId
        {
            get => GetUrlParameter("orderid");
            set => this.QueryString.Add("orderId", value);
        }

        public string Scenario
        {
            get => GetUrlParameter("scenario");
            set => this.QueryString.Add("scenario", value);
        }

        public string Partner
        {
            get => GetUrlParameter("partner");
            set => this.QueryString.Add("partner", value);
        }

        public string PaymentSessionOrData
        {
            get => GetUrlParameter("paymentSessionOrData");
            set => this.QueryString.Add("paymentSessionOrData", value);
        }

        public string Operation
        {
            get => GetUrlParameter("operation");
            set => this.QueryString.Add("operation", value);
        }

        public string Country
        {
            get => GetUrlParameter("country");
            set => this.QueryString.Add("country", value);
        }

        public string Language
        {
            get => GetUrlParameter("language");
            set => this.QueryString.Add("language", value);
        }

        public string CompletePrerequisites
        {
            get => GetUrlParameter("completePrerequisites");
            set => this.QueryString.Add("completePrerequisites", value);
        }

        public PXServiceRequestBuilder(Uri baseUri)
        {
            this.baseUri = baseUri;
            QueryString = System.Web.HttpUtility.ParseQueryString(string.Empty);
        }

        public string GetRequestUri(string area)
        {
            string fullUrl = $"{baseUri}/v7.0/{this.AccountId}/{area}";

            if (this.QueryString.HasKeys())
            {
                fullUrl += "?" + this.QueryString.ToString();
            }
            
            return new Uri(fullUrl).AbsoluteUri;
        }
        
        private string GetUrlParameter(string name)
        {
            var key = this.QueryString.AllKeys.FirstOrDefault(k => string.Equals(k, name, StringComparison.OrdinalIgnoreCase));
            if (key == null)
            {
                return null;
            }

            return this.QueryString.Get(key);
        }

        public NameValueCollection QueryString { get; private set; }
    }
}