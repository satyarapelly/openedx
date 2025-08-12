// <copyright file="LegacyAccountHelper.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Threading.Tasks;
    using Microsoft.Commerce.Payments.Common;
    using Microsoft.Commerce.Payments.Common.Tracing;
    using Microsoft.Commerce.Payments.Common.Web;
    using Microsoft.Commerce.Payments.PidlFactory.V7;
    using Microsoft.Commerce.Payments.PXService.Accessors.LegacyCommerceService;
    using Microsoft.Commerce.Payments.PXService.Accessors.LegacyCommerceService.DataModel;
    using Microsoft.Commerce.Payments.PXService.Accessors.LegacyCommerceService.Messages;
    using Microsoft.Commerce.Payments.PXService.Settings;
    using Pidl.Localization;

    public class LegacyAccountHelper
    {
        private const int InternalServerErrorCode = 10015;

        private static List<string> allAddressFields = new List<string>()
        {
            GlobalConstants.AddressErrorTargets.AddressLine1,
            GlobalConstants.AddressErrorTargets.AddressLine2,
            GlobalConstants.AddressErrorTargets.AddressLine3,
            GlobalConstants.AddressErrorTargets.State,
            GlobalConstants.AddressErrorTargets.City,
            GlobalConstants.AddressErrorTargets.PostalCode
        };

        private static List<string> stateCityZipFields = new List<string>()
        {
            GlobalConstants.AddressErrorTargets.State,
            GlobalConstants.AddressErrorTargets.City,
            GlobalConstants.AddressErrorTargets.PostalCode
        };

        private static Dictionary<string, Tuple<string, string, List<string>>> legacyAccountServiceErrors = new Dictionary<string, Tuple<string, string, List<string>>>()
        {
            // BDK_E_REQUIRED_FIELD_MISSING
            { "40005", Tuple.Create(GlobalConstants.LegacyAccountErrorCodes.InvalidAddress, GlobalConstants.LegacyAccountErrorMessages.InvalidAddressRequiredFieldMissing, allAddressFields) },

            // ACCOUNT_E_INVALID_ARGUMENT
            { "337681", Tuple.Create(GlobalConstants.LegacyAccountErrorCodes.InvalidAddress, GlobalConstants.LegacyAccountErrorMessages.InvalidAddress, allAddressFields) },

            // BDK_E_INVALID_PARAMETER
            { "44199", Tuple.Create(GlobalConstants.LegacyAccountErrorCodes.InvalidAddress, GlobalConstants.LegacyAccountErrorMessages.InvalidAddress, allAddressFields) },

            // BDK_E_INVALID_ADDRESS_FIELD_LENGTH
            { "10033", Tuple.Create(GlobalConstants.LegacyAccountErrorCodes.InvalidAddress, GlobalConstants.LegacyAccountErrorMessages.InvalidAddress, allAddressFields) },

            // BDK_E_ADDRESS_INVALID_FIELD_VALUE
            { "60001", Tuple.Create(GlobalConstants.LegacyAccountErrorCodes.InvalidAddress, GlobalConstants.LegacyAccountErrorMessages.InvalidAddress, allAddressFields) },

            // BDK_E_BADZIP
            { "10021", Tuple.Create(GlobalConstants.LegacyAccountErrorCodes.InvalidZipCode, GlobalConstants.LegacyAccountErrorMessages.InvalidZipCode, new List<string> { GlobalConstants.AddressErrorTargets.PostalCode }) },

            // BDK_E_STATE_INVALID
            { "60012", Tuple.Create(GlobalConstants.LegacyAccountErrorCodes.InvalidState, GlobalConstants.LegacyAccountErrorMessages.InvalidState, new List<string> { GlobalConstants.AddressErrorTargets.State }) },

            // BDK_E_STATE_ZIP_CITY_INVALID
            { "60016", Tuple.Create(GlobalConstants.LegacyAccountErrorCodes.InvalidAddress, GlobalConstants.LegacyAccountErrorMessages.InvalidAddress, stateCityZipFields) },

            // BDK_E_STATE_ZIP_CITY_INVALID2
            { "60017", Tuple.Create(GlobalConstants.LegacyAccountErrorCodes.InvalidAddress, GlobalConstants.LegacyAccountErrorMessages.InvalidAddress, stateCityZipFields) },

            // BDK_E_STATE_ZIP_CITY_INVALID3
            { "60018", Tuple.Create(GlobalConstants.LegacyAccountErrorCodes.InvalidAddress, GlobalConstants.LegacyAccountErrorMessages.InvalidAddress, stateCityZipFields) },

            // BDK_E_STATE_ZIP_CITY_INVALID4
            { "60019", Tuple.Create(GlobalConstants.LegacyAccountErrorCodes.InvalidAddress, GlobalConstants.LegacyAccountErrorMessages.InvalidAddress, stateCityZipFields) },

            // BDK_E_ZIP_INVALID
            { "60011", Tuple.Create(GlobalConstants.LegacyAccountErrorCodes.InvalidZipCode, GlobalConstants.LegacyAccountErrorMessages.InvalidZipCode, new List<string> { GlobalConstants.AddressErrorTargets.PostalCode }) },

            // BDK_E_ZIP_CITY_MISSING
            { "60071", Tuple.Create(GlobalConstants.LegacyAccountErrorCodes.InvalidZipCode, GlobalConstants.LegacyAccountErrorMessages.InvalidZipCode, new List<string> { GlobalConstants.AddressErrorTargets.City, GlobalConstants.AddressErrorTargets.PostalCode }) },

            // BDK_E_STATE_ZIP_INVALID
            { "60014", Tuple.Create(GlobalConstants.LegacyAccountErrorCodes.InvalidAddress, GlobalConstants.LegacyAccountErrorMessages.InvalidAddress, stateCityZipFields) },

            // BDK_E_STATE_CITY_INVALID
            { "60015", Tuple.Create(GlobalConstants.LegacyAccountErrorCodes.InvalidAddress, GlobalConstants.LegacyAccountErrorMessages.InvalidAddress, stateCityZipFields) },

            // BDK_E_MULTIPLE_COUNTIES_FOUND
            { "60029", Tuple.Create(GlobalConstants.LegacyAccountErrorCodes.InvalidZipCode, GlobalConstants.LegacyAccountErrorMessages.InvalidZipCode, new List<string> { GlobalConstants.AddressErrorTargets.PostalCode }) },

            // BDK_E_ZIP_INVALID_FOR_ENTERED_STATE
            { "60030", Tuple.Create(GlobalConstants.LegacyAccountErrorCodes.InvalidAddress, GlobalConstants.LegacyAccountErrorMessages.InvalidAddress, stateCityZipFields) },

            // BDK_E_STATE_ZIP_COVERS_MULTIPLE_CITIES
            { "60041", Tuple.Create(GlobalConstants.LegacyAccountErrorCodes.InvalidAddress, GlobalConstants.LegacyAccountErrorMessages.InvalidAddress, stateCityZipFields) },

            // BDK_E_MULTIPLE_CITIES_FOUND
            { "60042", Tuple.Create(GlobalConstants.LegacyAccountErrorCodes.InvalidZipCode, GlobalConstants.LegacyAccountErrorMessages.InvalidZipCode, new List<string> { GlobalConstants.AddressErrorTargets.PostalCode }) },

            // BDK_E_BAD_STATECODE_LENGTH
            { "60045", Tuple.Create(GlobalConstants.LegacyAccountErrorCodes.InvalidState, GlobalConstants.LegacyAccountErrorMessages.InvalidState, new List<string> { GlobalConstants.AddressErrorTargets.State }) }
        };

        public static void UpdateLegacyBillableAccountAddress(
            PXServiceSettings pxServiceSettings, 
            string billableAccountId, 
            PIDLData pi, 
            EventTraceActivity traceActivityId, 
            string altSecId, 
            string orgPuid, 
            string ipAddress, 
            string language)
        {
            if (string.IsNullOrEmpty(billableAccountId))
            {
                throw TraceCore.TraceException(traceActivityId, new ValidationException(ErrorCode.InvalidRequestData, string.Format("response status code: {0}, error: {1}", "InvalidPIData", "billableAccountId is required for Azure add/update PI scenario.")));
            }
            
            PayinPayoutAccount account = GetLegacyBillableAccountFromId(pxServiceSettings, billableAccountId, traceActivityId, altSecId, orgPuid, language);

            // Only update account if address set on PayIn account is empty
            if (account.PayinAccount.AddressSet == null || account.PayinAccount.AddressSet.Count == 0)
            {
                // For pay in account updating, don't touch anything in payout account
                // also, remove currency info from PayinAccount
                account.PayoutAccount = null;
                account.PayinAccount.Currency = null;

                // Remove LastName and Email from PayinAccount so that they won't be changed during update call. Refer bug #26423082
                account.PayinAccount.LastName = null;
                account.PayinAccount.Email = null;

                // Set TaxExemptionSet to null if it is for BrazilCPFID. Refer bug #26087005
                if (account.PayinAccount.TaxExemptionSet != null
                    && account.PayinAccount.TaxExemptionSet.Any(i => i.TaxExemptionType == TaxExemptionType.BrazilCPFID))
                {
                    account.PayinAccount.TaxExemptionSet = null;
                }

                // Extract address from PI Data and construct an legacy commerce service address object
                account.PayinAccount.AddressSet = new List<Address>();
                Address accountAddress = new Address()
                {
                    Street1 = pi.TryGetPropertyValue("details.address.address_line1"),
                    Street2 = pi.TryGetPropertyValue("details.address.address_line2"),
                    Street3 = pi.TryGetPropertyValue("details.address.address_line3"),
                    City = pi.TryGetPropertyValue("details.address.city"),
                    State = pi.TryGetPropertyValue("details.address.region"),
                    CountryCode = pi.TryGetPropertyValue("details.address.country"),
                    PostalCode = pi.TryGetPropertyValue("details.address.postal_code"),
                    FriendlyName = "My Address" // Friendly name (hard-coded) for the address as needed by Account Service
                };
                account.PayinAccount.AddressSet.Add(accountAddress);

                var fraudDetectionProperties = GetFraudDetectionContextProperties(pi.TryGetPropertyValue("riskData.greenId"), GlobalConstants.PartnerGuids.Azure, ipAddress);

                string identityValue;
                string identityType;
                CommerceHelper.GetIdentity(altSecId, orgPuid, traceActivityId, out identityValue, out identityType);

                UpdateLegacyBillableAccount(pxServiceSettings, identityType, identityValue, billableAccountId, account, GlobalConstants.PartnerGuids.Azure, fraudDetectionProperties, traceActivityId, language);
            }
        }

        public static PayinPayoutAccount GetLegacyBillableAccountFromId(PXServiceSettings pxServiceSettings, string billableAccountId, EventTraceActivity traceActivityId, string altSecId, string orgPuid, string language)
        {
            if (string.IsNullOrEmpty(billableAccountId))
            {
                throw TraceCore.TraceException(traceActivityId, new ValidationException(ErrorCode.InvalidRequestData, string.Format("response status code: {0}, error: {1}", "InvalidPIData", "billableAccountId is required for Azure add/update PI scenario.")));
            }

            string identityValue;
            string identityType;
            CommerceHelper.GetIdentity(altSecId, orgPuid, traceActivityId, out identityValue, out identityType);

            List<PayinPayoutAccount> list = GetLegacyBillableAccounts(pxServiceSettings, identityType, identityValue, traceActivityId, language, billableAccountId);
            if (list == null || list.Count == 0)
            {
                throw TraceCore.TraceException(traceActivityId, new InvalidOperationException(string.Format("Merged PayIn PayOut Account is empty for AccountId: {0}", billableAccountId)));
            }

            PayinPayoutAccount account = list[0];

            // Azure should be providing a Business type of PayIn account which is Active
            if (account.PayinAccount == null || account.PayinAccount.CustomerType == CustomerType.Personal || !CheckAccountStatus(account, AccountStatus.Active))
            {
                throw TraceCore.TraceException(traceActivityId, new InvalidOperationException(string.Format("Valid Legacy Billable PayIn Account is not found for AccountId: {0}", billableAccountId)));
            }

            return account;
        }

        public static async Task<PayinPayoutAccount> GetPersonalLegacyBillableAccountFromMarket(PXServiceSettings pxServiceSettings, EventTraceActivity traceActivityId, string puid, string language, string country, string accountId)
        {
            string identityValue;
            string identityType;
            CommerceHelper.GetIdentity(puid, null, traceActivityId, out identityValue, out identityType);

            List<PayinPayoutAccount> mergedAccountList = GetLegacyBillableAccounts(pxServiceSettings, identityType, identityValue, traceActivityId, language, null, true);
            List<PayinPayoutAccount> filteredAccountList = mergedAccountList.Where(account => IsBelongToCountry(account, country)).ToList();

            // Create a legacy billable account if an account is not found for the country
            if (filteredAccountList.Count == 0)
            {
                var legacyBillableAccount = await pxServiceSettings.AccountServiceAccessor.GetOrCreateLegacyBillableAccount(accountId, country, traceActivityId);
                mergedAccountList = GetLegacyBillableAccounts(pxServiceSettings, identityType, identityValue, traceActivityId, language, null, true);
                filteredAccountList = mergedAccountList.Where(account => IsBelongToCountry(account, country)).ToList();
            }

            // Since it is a personal account, we check wether all the primary accounts are locked, if yes, then throw exception.
            // Get all the personal accounts
            List<PayinPayoutAccount> personalAccounts = filteredAccountList.Where(account => IsPersonalAccount(account)).ToList();

            // Get all the active personal accounts ("active" means at least one of the payin and payout accounts is in active status)
            List<PayinPayoutAccount> activePersonalAccounts = personalAccounts.Where(account => CheckAccountStatus(account, AccountStatus.Active)).ToList();

            // Get all the locked personal accounts ("locked" means both of the payin and payout accounts (if exsited) are in locked status)
            List<PayinPayoutAccount> lockedPersonalAccounts = personalAccounts.Where(account => CheckAccountStatus(account, AccountStatus.Locked)).ToList();

            if (activePersonalAccounts.Count<PayinPayoutAccount>() == 0 && lockedPersonalAccounts.Count<PayinPayoutAccount>() != 0)
            {
                throw TraceCore.TraceException(traceActivityId, new InvalidOperationException(string.Format("No active primary accounts")));
            }

            // We allow only valid/active account.
            IEnumerable<PayinPayoutAccount> accounts = filteredAccountList.Where(account => CheckAccountStatus(account, AccountStatus.Active));

            List<PayinPayoutAccount> personalPayinPayout = new List<PayinPayoutAccount>();
            List<PayinPayoutAccount> personalPayinOnly = new List<PayinPayoutAccount>();

            foreach (PayinPayoutAccount acct in accounts)
            {
                if (acct.PayinAccount != null && acct.PayoutAccount != null)
                {
                    if (acct.PayinAccount.CustomerType == CustomerType.Personal)
                    {
                        personalPayinPayout.Add(acct);
                    }
                }
                else if (acct.PayinAccount != null)
                {
                    if (acct.PayinAccount.CustomerType == CustomerType.Personal)
                    {
                        personalPayinOnly.Add(acct);
                    }
                }
            }

            PayinPayoutAccount selectedAccount = null;
            if (personalPayinPayout.Count != 0)
            {
                selectedAccount = personalPayinPayout[0];
            }
            else if (personalPayinOnly.Count != 0)
            {
                selectedAccount = personalPayinOnly[0];
            }

            if (!CheckAccountStatus(selectedAccount, AccountStatus.Active))
            {
                throw TraceCore.TraceException(traceActivityId, new InvalidOperationException(string.Format("Invalid account status, accountId:{0}", selectedAccount.AccountID)));
            }

            return selectedAccount;
        }

        public static List<string> GetLegacyBillablePayinAccountIds(PXServiceSettings pxServiceSettings, EventTraceActivity traceActivityId, string puid, string language)
        {
            string identityValue;
            string identityType;
            CommerceHelper.GetIdentity(puid, null, traceActivityId, out identityValue, out identityType);

            var billableAccounts = GetLegacyBillableAccounts(pxServiceSettings, identityType, identityValue, traceActivityId, language);
            var payinAccounts = billableAccounts.Where(account => account?.PayinAccount != null && account.PayinAccount.Status.HasValue && account.PayinAccount.Status != AccountStatus.Closed).ToList();

            return payinAccounts.Select(payinAccount => payinAccount.AccountID).ToList();
        }

        public static string GetBillableAccountId(string piid, EventTraceActivity traceActivityId)
        {
            string billableAccountId = null;
            Common.Instruments.BdkId piBillableAccount = null;
            if (Common.Instruments.BdkId.TryParse(piid, traceActivityId, out piBillableAccount))
            {
                billableAccountId = new Common.Instruments.BdkId(piBillableAccount.AccountId).ToString();
            }

            return billableAccountId;
        }

        private static bool IsPersonalAccount(PayinPayoutAccount account)
        {
            if (account == null)
            {
                return false;
            }

            if (account.PayinAccount != null)
            {
                return account.PayinAccount.CustomerType == CustomerType.Personal;
            }
            else
            {
                return false;
            }
        }

        private static bool IsBelongToCountry(PayinPayoutAccount account, string country)
        {
            if (account.PayinAccount != null)
            {
                return string.Compare(account.PayinAccount.CountryCode, country, true) == 0;
            }
            else if (account.PayoutAccount != null)
            {
                return string.Compare(account.PayoutAccount.CountryCode, country, true) == 0;
            }

            return false;
        }

        // The below method 'MergeAccount' is a copy from SC.CSPayments.PCS/PCSV2/WebSite/Core/PCSDataAccessorManager.cs
        // This is to follow the same business logic for merging Payin / Payout acocunts as in PCS.
        private static List<PayinPayoutAccount> MergeAccount(List<Account> accounts, bool primaryAccountOnly = false)
        {
            List<PayinPayoutAccount> mergeAccountList = new List<PayinPayoutAccount>();
            PayinPayoutAccount mergedAccount = null;

            var query = accounts.GroupBy(account => account.AccountID).SelectMany(account => account);

            foreach (Account acct in query)
            {
                if (primaryAccountOnly
                    && (acct == null || !(acct.AccountLevel == "Primary")))
                {
                    // Account Unification.
                    // Allow only Primary account.
                    // 1 If(no primary)=>no account.
                    // 2 If(more than one primary) => take the first one as before.
                    //   If UA doesn't happen to a user, all of his/her accounts are primary
                    // 3 we should not pick secondary account.
                    continue;
                }

                if (mergedAccount == null || string.Compare(acct.AccountID, mergedAccount.AccountID, true) != 0)
                {
                    mergedAccount = new PayinPayoutAccount();
                    mergedAccount.AccountID = acct.AccountID;
                    mergeAccountList.Add(mergedAccount);
                }

                if (acct.GetType() == typeof(PayinAccount) && mergedAccount.PayinAccount == null)
                {
                    mergedAccount.PayinAccount = (PayinAccount)acct;
                }
                else if (acct.GetType() == typeof(PayoutAccount) && mergedAccount.PayoutAccount == null)
                {
                    mergedAccount.PayoutAccount = (PayoutAccount)acct;
                }
            }

            return mergeAccountList;
        }

        // The below method 'CheckAccountStatus' is a copy from SC.CSPayments.PCS/PCSV2/WebSite/Core/PCSDataAccessorManager.cs
        // This is to follow the same business logic for validating the account status as in PCS.
        private static bool CheckAccountStatus(PayinPayoutAccount account, AccountStatus status)
        {
            if (account == null)
            {
                if (status == AccountStatus.Active)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }

            // If either of the payin and payout account is active, then this account is active.
            // If neither of the payin nor payout account is active and both of them are locked, then this account is locked. 
            // If neither of the payin nor payout account is active and either of them is closed, then this account is closed.
            if (account.PayinAccount != null && account.PayoutAccount != null)
            {
                if (status == AccountStatus.Active &&
                    (account.PayinAccount.Status == status || account.PayoutAccount.Status == status))
                {
                    return true;
                }

                if (status == AccountStatus.Locked &&
                    (account.PayinAccount.Status == status && account.PayoutAccount.Status == status))
                {
                    return true;
                }

                if (status == AccountStatus.Closed &&
                    (account.PayinAccount.Status != AccountStatus.Active && account.PayoutAccount.Status != AccountStatus.Active) &&
                    (account.PayinAccount.Status == status || account.PayoutAccount.Status == status))
                {
                    return true;
                }
            }
            else if (account.PayinAccount == null)
            {
                if (account.PayoutAccount.Status == status)
                {
                    return true;
                }
            }
            else if (account.PayoutAccount == null)
            {
                if (account.PayinAccount.Status == status)
                {
                    return true;
                }
            }

            return false;
        }

        private static void AddProperty(List<Property> properties, string propName, string propValue)
        {
            if (!string.IsNullOrEmpty(propValue))
            {
                properties.Add(new Property { Namespace = GlobalConstants.Namespaces.Risk, Name = propName, Value = propValue });
            }
        }

        private static Exception ConvertDataAccessException(DataAccessException exception, string exceptionMessage, EventTraceActivity traceActivityId, string language)
        {
            if (exception.ErrorCode == DataAccessErrors.DATAACCESS_E_EXTERNAL_TIMEOUT_ERROR
                || exception.ErrorCode == DataAccessErrors.DATAACCESS_E_SERVICECALL_ERROR)
            {
                exceptionMessage += "  " + string.Format("Api Response: {0}", exception.TracerResult?.RawApiResponse);
                return TraceCore.TraceException(traceActivityId, new PXServiceException(exceptionMessage, GlobalConstants.PXServiceErrorCodes.LegacyAccountServiceFailed, exception));
            }

            var accountServiceError = new ServiceErrorResponse(exception.ErrorCode.ToString(), exception.Error?.ErrorLongMessage ?? "Unknown error");
            accountServiceError.Source = "LegacyAccountService";
            var pxServiceErrorResponse = new ServiceErrorResponse(traceActivityId.ActivityId.ToString(), GlobalConstants.ServiceName, accountServiceError)
            {
                HttpStatusCode = (exception.ErrorCode == InternalServerErrorCode) ? HttpStatusCode.InternalServerError : HttpStatusCode.BadRequest,
                ErrorCode = GlobalConstants.PXServiceErrorCodes.LegacyAccountServiceFailed,
                Message = GlobalConstants.ClientActionContract.NoMessage
            };

            Tuple<string, string, List<string>> errorMappingInfo;
            if (legacyAccountServiceErrors.TryGetValue(exception.ErrorCode.ToString(), out errorMappingInfo))
            {
                pxServiceErrorResponse.ErrorCode = errorMappingInfo.Item1;
                pxServiceErrorResponse.AddDetail(new ServiceErrorDetail()
                {
                    ErrorCode = errorMappingInfo.Item1,
                    Message = LocalizationRepository.Instance.GetLocalizedString(errorMappingInfo.Item2, language),
                    Target = string.Join(",", errorMappingInfo.Item3)
                });
            }

            var pxServiceResponseException = new ServiceErrorResponseException() { Error = pxServiceErrorResponse };
            return TraceCore.TraceException(traceActivityId, pxServiceResponseException);
        }

        private static List<PayinPayoutAccount> GetLegacyBillableAccounts(PXServiceSettings pxServiceSettings, string identityType, string identityValue, EventTraceActivity traceActivityId, string language, string accountId = null, bool primaryAccountOnly = false)
        {
            // Construct an object of GetAccountInfoRequest
            GetAccountInfoRequest getAccountRequest = new GetAccountInfoRequest()
            {
                APIContext = new APIContext()
                {
                    TrackingGuid = Guid.NewGuid(),
                },
                CallerInfo = new CallerInfo()
                {
                    Requester = new Identity()
                    {
                        IdentityType = identityType,
                        IdentityValue = identityValue
                    }
                }
            };

            if (!string.IsNullOrEmpty(accountId))
            {
                getAccountRequest.SearchCriteria = new AccountSearchCriteria
                {
                    AccountId = accountId
                };
            }
            else
            {
                getAccountRequest.SearchCriteria = new AccountSearchCriteria
                {
                    Identity = getAccountRequest.CallerInfo.Requester
                };
            }

            // Make a Get Account call to account service
            GetAccountInfoResponse getAccountResponse = null;
            try
            {
                getAccountResponse = pxServiceSettings.CommerceAccountDataServiceAccessor.GetAccountInfo(getAccountRequest, traceActivityId);
                if (getAccountResponse == null || getAccountResponse.AccountList == null)
                {
                    throw TraceCore.TraceException(traceActivityId, new InvalidOperationException(string.Format("Legacy Billable Account is not found for AccountId: {0}", accountId)));
                }
            }
            catch (DataAccessException exception)
            {
                string exceptionMessage = string.Format("GetAccountInfo failed for AccountId: {0}. Legacy Account Service exception: {1}.", accountId, exception.ToString());
                throw ConvertDataAccessException(exception, exceptionMessage, traceActivityId, language);
            }

            // Combine PayinAccount and PayoutAccount with same AccountID into one account PayinPayoutAccount
            return MergeAccount(getAccountResponse.AccountList, primaryAccountOnly);
        }

        private static void UpdateLegacyBillableAccount(PXServiceSettings pxServiceSettings, string identityType, string identityValue, string accountId, PayinPayoutAccount account, string partnerGuid, List<Property> fraudDetectionProperties, EventTraceActivity traceActivityId, string language)
        {
            // Construct an object of UpdateAccountRequest
            UpdateAccountRequest updateAccountRequest = new UpdateAccountRequest()
            {
                APIContext = new APIContext()
                {
                    TrackingGuid = Guid.NewGuid(),
                    FraudDetectionContext = fraudDetectionProperties
                },
                CallerInfo = new CallerInfo()
                {
                    AccountId = accountId,
                    Requester = new Identity()
                    {
                        IdentityType = identityType,
                        IdentityValue = identityValue
                    }
                },
                OnBehalfOfPartner = new Guid(partnerGuid),
                Account = account,
            };

            // Make an update account request
            try
            {
                UpdateAccountResponse updateAccountResponse = pxServiceSettings.CommerceAccountDataServiceAccessor.UpdateAccount(updateAccountRequest, traceActivityId);
                if (updateAccountResponse == null)
                {
                    throw TraceCore.TraceException(traceActivityId, new PXServiceException(string.Format("Legacy Billable Account could not be updated for AccountId: {0}", accountId), GlobalConstants.PXServiceErrorCodes.LegacyBillableAccountUpdateFailed));
                }
            }
            catch (DataAccessException exception)
            {
                string exceptionMessage = string.Format("UpdateAccount failed for AccountId: {0}. Legacy Account Service exception: {1}.", accountId, exception.ToString());
                throw ConvertDataAccessException(exception, exceptionMessage, traceActivityId, language);
            }
        }

        private static List<Property> GetFraudDetectionContextProperties(string greenId, string partnerGuid, string ipAddress)
        {
            var properties = new List<Property>();

            // Property names to match with PCS, so that the existing RISK rules can work
            AddProperty(properties, "THM_SESSION_ID", greenId);
            AddProperty(properties, "PCSPartnerName", "azurev2");
            AddProperty(properties, "OnBehalfOfPartnerGuid", partnerGuid);
            AddProperty(properties, "PCSActionParam", "NORMAL");
            AddProperty(properties, "IPAddress", ipAddress);

            return properties;
        }
    }
}