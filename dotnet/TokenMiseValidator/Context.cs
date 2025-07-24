using System.Globalization;
using System.Threading;
using Microsoft.AspNetCore.Http;
using PXCommon;

namespace Microsoft.Commerce.Payments.PidlModel
{
    /// <summary>
    /// Provides request specific context such as country and culture.
    /// Works in ASP.NET Core using <see cref="IHttpContextAccessor"/> and
    /// falls back to <see cref="AsyncLocal{T}"/> values when running
    /// in a self-hosted environment.
    /// </summary>
    public static class Context
    {
        private const string COUNTRY = "UserCountry";
        private const string EMAIL = "UserEmailAddress";
        private const string CULTURE = "UserCultureInfo";
        private const string PARTNERNAME = "UserPartner";

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
