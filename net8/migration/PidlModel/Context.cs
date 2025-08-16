// <copyright file="Context.cs" company="Microsoft">Copyright (c) Microsoft 2015. All rights reserved.</copyright>
namespace Microsoft.Commerce.Payments.PidlModel
{
    using PXCommon;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Resources;
    using System.Threading;
    using System.Web;
    using PXCommon;

    /// <summary>
    /// The PIDL Model holds template data that is language and country neutral but 
    /// projects a view (via property getters) that is language and country appropriate.
    /// This class holds such information as a global context for PIDLModel.
    /// </summary>
    public static class Context
    {
        private const string COUNTRY = "UserCountry";
        private const string EMAIL = "UserEmailAddress";
        private const string CULTURE = "UserCultureInfo";
        private const string PARTNERNAME = "UserPartner";

        //// AsyncLocal<T> variables are only being used when running in selfhost mode.
        //// Only PXService.CITs and DiffTest use these.
        private static AsyncLocal<CultureInfo> culture = new AsyncLocal<CultureInfo>();
        
        private static AsyncLocal<string> emailAddress = new AsyncLocal<string>();

        private static AsyncLocal<string> country = new AsyncLocal<string>();

        private static AsyncLocal<string> partnerName = new AsyncLocal<string>();

        public static string Country
        {
            get
            {
                if (HttpContext.Current != null && HttpContext.Current.Items != null)
                {
                    return HttpContext.Current.Items[COUNTRY] as string;
                }

                if (WebHostingUtility.IsApplicationSelfHosted())
                {
                    return country.Value;
                }

                return null;
            }

            set
            {
                if (HttpContext.Current != null && HttpContext.Current.Items != null)
                {
                    HttpContext.Current.Items[COUNTRY] = value;
                }

                if (WebHostingUtility.IsApplicationSelfHosted())
                {
                    country.Value = value;
                }
            }
        }

        public static string PartnerName
        {
            get
            {
                if (HttpContext.Current != null && HttpContext.Current.Items != null)
                {
                    return HttpContext.Current.Items[PARTNERNAME] as string;
                }

                if (WebHostingUtility.IsApplicationSelfHosted())
                {
                    return partnerName.Value;
                }

                return null;
            }

            set
            {
                if (HttpContext.Current != null && HttpContext.Current.Items != null)
                {
                    HttpContext.Current.Items[PARTNERNAME] = value;
                }

                if (WebHostingUtility.IsApplicationSelfHosted())
                {
                    partnerName.Value = value;
                }
            }
        }

        public static string EmailAddress
        {
            get
            {
                if (HttpContext.Current != null && HttpContext.Current.Items != null)
                {
                    return HttpContext.Current.Items[EMAIL] as string;
                }

                if (WebHostingUtility.IsApplicationSelfHosted())
                {
                    return emailAddress.Value;
                }

                return null;
            }

            set
            {
                if (HttpContext.Current != null && HttpContext.Current.Items != null)
                {
                    HttpContext.Current.Items[EMAIL] = value;
                }

                if (WebHostingUtility.IsApplicationSelfHosted())
                {
                    emailAddress.Value = value;
                }
            }
        }

        public static CultureInfo Culture
        {
            get
            {
                if (HttpContext.Current != null && HttpContext.Current.Items != null)
                {
                    return HttpContext.Current.Items[CULTURE] as CultureInfo;
                }

                if (WebHostingUtility.IsApplicationSelfHosted())
                {
                    return culture.Value;
                }

                return null;
            }

            set
            {
                if (HttpContext.Current != null && HttpContext.Current.Items != null)
                {
                    HttpContext.Current.Items[CULTURE] = value;
                }

                if (WebHostingUtility.IsApplicationSelfHosted())
                {
                    culture.Value = value;
                }
            }
        }
    }
}
