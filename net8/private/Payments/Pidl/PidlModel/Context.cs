// <copyright file="Context.cs" company="Microsoft">Copyright (c) Microsoft 2015. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PidlModel
{
	using Microsoft.AspNetCore.Http;
	using Microsoft.Commerce.Payments.PXCommon;
	using System.Globalization;
	using System.Threading;

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
        private static readonly AsyncLocal<CultureInfo?> culture = new();
        private static readonly AsyncLocal<string?> emailAddress = new();
        private static readonly AsyncLocal<string?> country = new();
        private static readonly AsyncLocal<string?> partnerName = new();

        /// <summary>
        /// Accessor used to obtain the current <see cref="HttpContext"/>.
        /// This should be configured by the host application.
        /// </summary>
        public static IHttpContextAccessor? HttpContextAccessor { get; set; }

        private static HttpContext? Current => HttpContextAccessor?.HttpContext;

        public static string? Country
        {
            get
            {
                if (Current?.Items != null && Current.Items.TryGetValue(COUNTRY, out var val))
                {
                    return val as string;
                }

                if (WebHostingUtility.IsApplicationSelfHosted())
                {
                    return country.Value;
                }

                return null;
            }

            set
            {
                if (Current?.Items != null)
                {
                    Current.Items[COUNTRY] = value!;
                }

                if (WebHostingUtility.IsApplicationSelfHosted())
                {
                    country.Value = value;
                }
            }
        }

        public static string? PartnerName
        {
            get
            {
                if (Current?.Items != null && Current.Items.TryGetValue(PARTNERNAME, out var val))
                {
                    return val as string;
                }

                if (WebHostingUtility.IsApplicationSelfHosted())
                {
                    return partnerName.Value;
                }

                return null;
            }

            set
            {
                if (Current?.Items != null)
                {
                    Current.Items[PARTNERNAME] = value!;
                }

                if (WebHostingUtility.IsApplicationSelfHosted())
                {
                    partnerName.Value = value;
                }
            }
        }

        public static string? EmailAddress
        {
            get
            {
                if (Current?.Items != null && Current.Items.TryGetValue(EMAIL, out var val))
                {
                    return val as string;
                }

                if (WebHostingUtility.IsApplicationSelfHosted())
                {
                    return emailAddress.Value;
                }

                return null;
            }

            set
            {
                if (Current?.Items != null)
                {
                    Current.Items[EMAIL] = value!;
                }

                if (WebHostingUtility.IsApplicationSelfHosted())
                {
                    emailAddress.Value = value;
                }
            }
        }

        public static CultureInfo? Culture
        {
            get
            {
                if (Current?.Items != null && Current.Items.TryGetValue(CULTURE, out var val))
                {
                    return val as CultureInfo;
                }

                if (WebHostingUtility.IsApplicationSelfHosted())
                {
                    return culture.Value;
                }

                return null;
            }

            set
            {
                if (Current?.Items != null)
                {
                    Current.Items[CULTURE] = value!;
                }

                if (WebHostingUtility.IsApplicationSelfHosted())
                {
                    culture.Value = value;
                }
            }
        }
    }
}
