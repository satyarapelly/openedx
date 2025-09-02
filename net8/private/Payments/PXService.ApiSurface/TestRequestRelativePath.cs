// <copyright file="TestRequestRelativePath.cs" company="Microsoft">Copyright (c) Microsoft 2016. All rights reserved.</copyright>
namespace Microsoft.Commerce.Payments.PXService.ApiSurface.Diff
{
    using System.Collections.Specialized;
    using System.Web;

    public class TestRequestRelativePath
    {
        private NameValueCollection query;

        public TestRequestRelativePath(
            string userType,
            string resourceName,
            string country = null,
            string type = null,
            string language = null,
            string partner = null,
            string operation = null,
            string scenario = null,
            string family = null,
            string allowedPaymentMethods = null,
            string filters = null,
            string completePrerequisites = null,
            string merchantId = null,
            string currency = null,
            string paymentSessionOrData = null,
            string timezoneOffSet = null,
            string piid = null,
            string sessionId = null,
            bool avsSuggest = false,
            bool setAsDefaultBilling = false,
            string orderId = null,
            string channel = null,
            string referrerId = null,
            string redirectUrl = null,
            string paymentProviderId = null,
            string checkoutId = null,
            string rewardsContextData = null,
            string ocid = null,
            string expressCheckoutData = null)
        {
            this.query = HttpUtility.ParseQueryString(string.Empty);

            this.UserType = userType;
            this.ResourceName = resourceName;

            if (country != null)
            {
                this.query["country"] = country;
            }

            if (family != null)
            {
                this.query["family"] = family;
            }

            if (type != null)
            {
                this.query["type"] = type;
            }

            if (language != null)
            {
                this.query["language"] = language;
            }

            if (partner != null)
            {
                this.query["partner"] = partner;
            }

            if (operation != null)
            {
                this.query["operation"] = operation;
            }

            if (scenario != null)
            {
                this.query["scenario"] = scenario;
            }

            if (allowedPaymentMethods != null)
            {
                this.query["allowedPaymentMethods"] = allowedPaymentMethods;
            }

            if (filters != null)
            {
                this.query["filters"] = filters;
            }

            if (completePrerequisites != null)
            {
                this.query["completePrerequisites"] = completePrerequisites;
            }

            if (merchantId != null)
            {
                this.query["merchantId"] = merchantId;
            }

            if (currency != null)
            {
                this.query["currency"] = currency;
            }

            if (paymentSessionOrData != null)
            {
                this.query["paymentSessionOrData"] = paymentSessionOrData;
            }

            if (timezoneOffSet != null)
            {
                this.query["timezoneOffSet"] = timezoneOffSet;
            }

            if (piid != null)
            {
                this.query["piid"] = piid;
            }

            if (sessionId != null)
            {
                this.query["sessionId"] = sessionId;
            }

            if (avsSuggest)
            {
                this.query["avsSuggest"] = avsSuggest.ToString();
            }

            if (setAsDefaultBilling)
            {
                this.query["setAsDefaultBilling"] = setAsDefaultBilling.ToString();
            }

            if (!string.IsNullOrEmpty(orderId))
            {
                this.query["orderId"] = orderId;
            }

            if (!string.IsNullOrEmpty(channel))
            {
                this.query["channel"] = channel;
            }

            if (!string.IsNullOrEmpty(referrerId))
            {
                this.query["referrerId"] = referrerId;
            }

            if (!string.IsNullOrEmpty(redirectUrl))
            {
                this.query["redirectUrl"] = redirectUrl;
            }

            if (!string.IsNullOrEmpty(paymentProviderId))
            {
                this.query["paymentProviderId"] = paymentProviderId;
            }

            if (!string.IsNullOrEmpty(checkoutId))
            {
                this.query["checkoutId"] = checkoutId;
            }

            if (!string.IsNullOrEmpty(rewardsContextData))
            {
                this.query["rewardsContextData"] = rewardsContextData;
            }

            if (!string.IsNullOrEmpty(ocid))
            {
                this.query["ocid"] = ocid;
            }

            if (!string.IsNullOrEmpty(expressCheckoutData))
            {
                this.query["expressCheckoutData"] = expressCheckoutData;
            }
        }

        public TestRequestRelativePath(string userType, string resourceName, NameValueCollection query)
        {
            this.UserType = userType;
            this.ResourceName = resourceName;
            this.query = query;
        }

        public string UserType
        {
            get;
            private set;
        }

        public string ResourceName
        {
            get;
            private set;
        }

        public string Country
        {
            get
            {
                return this.query["country"];
            }
        }

        public string PIFamily
        {
            get
            {
                return this.query["family"];
            }
        }

        public string PaymentSessionOrData
        {
            get
            {
                return this.query["paymentSessionOrData"];
            }
        }

        public string PIType
        {
            get
            {
                return this.query["type"];
            }
        }

        public string Language
        {
            get
            {
                return this.query["language"];
            }
        }

        public string Partner
        {
            get
            {
                return this.query["partner"];
            }
        }

        public string Operation
        {
            get
            {
                return this.query["operation"];
            }
        }

        public string Scenario
        {
            get
            {
                return this.query["scenario"];
            }
        }

        public string AllowedPaymentMethods
        {
            get
            {
                return this.query["allowedPaymentMethod"];
            }
        }

        public string Filters
        {
            get
            {
                return this.query["filters"];
            }
        }

        public NameObjectCollectionBase.KeysCollection Keys
        {
            get { return this.query.Keys; }
        }

        public string this[string s]
        {
            get { return this.query[s]; }
        }

        public PidlIdentity GetPidlIdentity()
        {
            return new PidlIdentity()
            {
                UserType = this.UserType,
                ResourceName = this.ResourceName,
                Id = string.Format("{0}.{1}", this.PIFamily, this.PIType),
                Country = (this.Country != null) ? this.Country : string.Empty,
                Language = (this.Language != null) ? this.Language : string.Empty,
                Operation = (this.Operation != null) ? this.Operation : string.Empty,
                Partner = (this.Partner != null) ? this.Partner : string.Empty,
                Scenario = (this.Scenario != null) ? this.Scenario : string.Empty,
                Filters = (this.Filters != null) ? this.Filters : string.Empty,
                AllowedPayementMethods = (this.AllowedPaymentMethods != null) ? this.AllowedPaymentMethods : string.Empty
            };
        }

        public string ToString(bool localEnv, PIState state, string piid = null)
        {
            switch (state)
            {
                case PIState.None:
                case PIState.IssuerServiceApply:
                case PIState.Add:
                    return localEnv ? string.Format("{0}?{1}", this.ResourceName, HttpUtility.UrlDecode(this.query.ToString())) :
                        (this.UserType == Constants.UserTypes.Anonymous ? string.Empty : ("users/" + this.UserType + "/")) + string.Format("{0}?{1}", this.ResourceName, HttpUtility.UrlDecode(this.query.ToString()));
                case PIState.Get:
                    return localEnv ? string.Format("{0}/{1}?{2}", this.ResourceName, piid, HttpUtility.UrlDecode(this.query.ToString())) :
                        (this.UserType == Constants.UserTypes.Anonymous ? string.Empty : ("users/" + this.UserType + "/")) + string.Format("{0}/{1}?{2}", this.ResourceName, piid, HttpUtility.UrlDecode(this.query.ToString()));
                case PIState.Resume:
                    return localEnv ? string.Format("{0}/{1}/resume?{2}", this.ResourceName, piid, HttpUtility.UrlDecode(this.query.ToString())) :
                        (this.UserType == Constants.UserTypes.Anonymous ? string.Empty : ("users/" + this.UserType + "/")) + string.Format("{0}/{1}/resume?{2}", this.ResourceName, piid, HttpUtility.UrlDecode(this.query.ToString()));
            }

            return string.Empty;
        }
    }
}
