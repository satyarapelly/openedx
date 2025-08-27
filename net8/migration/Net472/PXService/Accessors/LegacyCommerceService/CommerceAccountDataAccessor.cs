// <copyright file="CommerceAccountDataAccessor.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Cryptography.X509Certificates;
    using Microsoft.Commerce.Payments.PXService.Accessors.LegacyCommerceService;
    using Microsoft.Commerce.Payments.PXService.Accessors.LegacyCommerceService.DataModel;
    using Microsoft.Commerce.Payments.PXService.Accessors.LegacyCommerceService.Messages;
    using Microsoft.Commerce.Proxy.AccountService;
    using Microsoft.Commerce.Tracing;
    using DataModel = Microsoft.Commerce.Payments.PXService.Accessors.LegacyCommerceService.DataModel;
    using ModelProperty = Microsoft.Commerce.Payments.PXService.Accessors.LegacyCommerceService.DataModel.Property;
    using ProxyProperty = Microsoft.Commerce.Proxy.AccountService.Property;

    public class CommerceAccountDataAccessor : CommerceServiceExecutor<AccountServiceChannel>, ICommerceAccountDataAccessor
    {
        private const string ServiceName = "LegacyAccountService";

        public CommerceAccountDataAccessor(string baseUrl, X509Certificate2 authCert) : base(baseUrl, authCert) { }

        protected override string ContractName
        {
            get { return "Microsoft.Commerce.Proxy.AccountService.AccountService"; }
        }

        public GetAccountInfoResponse GetAccountInfo(GetAccountInfoRequest request, EventTraceActivity traceActivityId)
        {
            return this.Execute<GetAccountInfoRequest, GetAccountInfoResponse, GetAccountInput, GetAccountOutput>
                (
                    DataAccessorType.GetAccountInfo,
                    request,
                    ConstructGetAccountInfoServiceInput,
                    (channel, input) =>
                    {
                        return channel.GetAccount(input);
                    },
                    ConstructGetAccountInfoOutput,
                    ServiceName,
                    traceActivityId
                );

        }

        public CreateAccountResponse CreateAccount(CreateAccountRequest request, EventTraceActivity traceActivityId)
        {
            return this.Execute<CreateAccountRequest, CreateAccountResponse, CreateAccountInput, CreateAccountOutput>
                (
                    DataAccessorType.CreateAccount,
                    request,
                    ConstructCreateAccountInput,
                    (channel, input) =>
                    {
                        return channel.CreateAccount(input);
                    },
                    ConstructCreateAccountOutput,
                    ServiceName,
                    traceActivityId
                );
        }

        public UpdateAccountResponse UpdateAccount(UpdateAccountRequest request, EventTraceActivity traceActivityId)
        {
            return this.Execute<UpdateAccountRequest, UpdateAccountResponse, UpdateAccountInput, UpdateAccountOutput>
                (
                    DataAccessorType.UpdateAccount,
                    request,
                    ConstructUpdateAccountInput,
                    (channel, input) =>
                    {
                        return channel.UpdateAccount(input);
                    },
                    ConstructUpdateAccountOutput,
                    ServiceName,
                    traceActivityId
                );
        }

        private static ProxyProperty[] ToProxyProperties(params List<ModelProperty>[] sourcePropertLists)
        {
            if (sourcePropertLists == null)
            {
                return null;
            }

            List<ProxyProperty> resultList = new List<ProxyProperty>();

            foreach (List<ModelProperty> list in sourcePropertLists)
            {
                if (list == null)
                {
                    continue;
                }

                list.ForEach(property =>
                {
                    if (property == null)
                    {
                        return;
                    }

                    resultList.Add(new ProxyProperty
                    {
                        Name = property.Name,
                        Namespace = property.Namespace,
                        Value = property.Value
                    });
                });
            }

            return resultList.ToArray();
        }

        private GetAccountInput ConstructGetAccountInfoServiceInput(GetAccountInfoRequest request)
        {
            if (request == null)
                return null;

            GetAccountInput input = new GetAccountInput();
            if (request.APIContext != null)
            {
                input.APIContext = new Microsoft.Commerce.Proxy.AccountService.APIContext();
                input.APIContext.TrackingGuid = request.APIContext.TrackingGuid;
            }

            if (request.CallerInfo != null)
            {
                if (request.CallerInfo.Requester != null)
                {
                    input.CallerInfo = new Microsoft.Commerce.Proxy.AccountService.CallerInfo();
                    input.CallerInfo.Requester = new Microsoft.Commerce.Proxy.AccountService.Identity();
                    input.CallerInfo.Requester.IdentityType = request.CallerInfo.Requester.IdentityType;
                    input.CallerInfo.Requester.IdentityValue = request.CallerInfo.Requester.IdentityValue;
                }
                if (request.CallerInfo.Delegator != null)
                {
                    input.CallerInfo.Delegator = new Microsoft.Commerce.Proxy.AccountService.Identity();
                    input.CallerInfo.Delegator.IdentityType = request.CallerInfo.Delegator.IdentityType;
                    input.CallerInfo.Delegator.IdentityValue = request.CallerInfo.Delegator.IdentityValue;
                }
            }

            if (request.SearchCriteria != null)
            {
                input.CallerInfo.AccountId = request.SearchCriteria.AccountId;
                input.SearchCriteria = new Microsoft.Commerce.Proxy.AccountService.AccountSearchCriteria();
                input.SearchCriteria.AccountId = request.SearchCriteria.AccountId;
                if (request.SearchCriteria.Identity != null)
                {
                    input.SearchCriteria.Identity = new Microsoft.Commerce.Proxy.AccountService.Identity();
                    input.SearchCriteria.Identity.IdentityType = request.SearchCriteria.Identity.IdentityType;
                    input.SearchCriteria.Identity.IdentityValue = request.SearchCriteria.Identity.IdentityValue;
                }
            }
            return input;
        }

        private GetAccountInfoResponse ConstructGetAccountInfoOutput(GetAccountOutput serviceOutput)
        {
            if (serviceOutput == null)
                return null;

            GetAccountInfoResponse response = new GetAccountInfoResponse();

            if (serviceOutput.Ack != Microsoft.Commerce.Proxy.AccountService.AckCodeType.Success)
            {
                throw new DataAccessException(
                    ErrorNamespace.Commerce,
                    serviceOutput.Error.ErrorCode,
                    serviceOutput.Error.ErrorShortMessage,
                    serviceOutput.Error.ErrorLongMessage,
                    serviceOutput.Error.ErrorDescription,
                    serviceOutput.Error.Retryable
                    );
            }

            response.Ack = Microsoft.Commerce.Payments.PXService.Accessors.LegacyCommerceService.DataModel.AckCodeType.Success;
            response.AccountList = TransformAccounts(serviceOutput.AccountOutputInfo);

            return response;
        }

        private CreateAccountInput ConstructCreateAccountInput(CreateAccountRequest request)
        {
            if (request == null)
                return null;

            CreateAccountInput input = new CreateAccountInput();
            if (request.APIContext != null)
            {
                input.APIContext = new Microsoft.Commerce.Proxy.AccountService.APIContext();
                input.APIContext.TrackingGuid = request.APIContext.TrackingGuid;
                // put risk properties in both Fraud and property bag because update account can't pass all
                // properties in FraudDetectionContect to risk.
                // properties in PropertBag should go to risk by RiskContext.PartnerInfo.PartnerCustomProperties
                input.APIContext.FraudDetectionContext = ToProxyProperties(request.APIContext.FraudDetectionContext);
                input.APIContext.PropertyBag = ToProxyProperties(request.APIContext.PropertyBag, request.APIContext.FraudDetectionContext);
            }

            if (request.CallerInfo != null)
            {
                if (request.CallerInfo.Requester != null)
                {
                    input.CallerInfo = new Microsoft.Commerce.Proxy.AccountService.CallerInfo();
                    input.CallerInfo.Requester = new Microsoft.Commerce.Proxy.AccountService.Identity();
                    input.CallerInfo.Requester.IdentityType = request.CallerInfo.Requester.IdentityType;
                    input.CallerInfo.Requester.IdentityValue = request.CallerInfo.Requester.IdentityValue;

                    switch (request.CallerInfo.Requester.IdentityType)
                    {
                        case "PUID":
                            input.CallerInfo.Requester.IdentityProperty = new Commerce.Proxy.AccountService.Property[1];
                            Commerce.Proxy.AccountService.Property property = new Commerce.Proxy.AccountService.Property();
                            input.CallerInfo.Requester.IdentityProperty[0] = property;
                            property.Name = "PassportMemberName";
                            property.Value = request.CallerInfo.Requester.PassportMemberName;
                            break;
                    }
                }
            }
            input.OnBehalfOfPartner = request.OnBehalfOfPartner;
            input.AccountInputInfo = BuildAccount(request.Account);

            return input;
        }

        private CreateAccountResponse ConstructCreateAccountOutput(CreateAccountOutput serviceOutput)
        {
            if (serviceOutput == null)
                return null;

            if (serviceOutput.Ack != Microsoft.Commerce.Proxy.AccountService.AckCodeType.Success)
            {
                throw new DataAccessException(
                    ErrorNamespace.Commerce,
                    serviceOutput.Error.ErrorCode,
                    serviceOutput.Error.ErrorShortMessage,
                    serviceOutput.Error.ErrorLongMessage,
                    serviceOutput.Error.ErrorDescription,
                    serviceOutput.Error.Retryable
                    );
            }

            CreateAccountResponse response = new CreateAccountResponse();
            response.Account = TransformAccount(serviceOutput.AccountOutputInfo);
            return response;
        }

        private UpdateAccountInput ConstructUpdateAccountInput(UpdateAccountRequest request)
        {
            if (request == null)
                return null;

            UpdateAccountInput input = new UpdateAccountInput();
            if (request.APIContext != null)
            {
                input.APIContext = new Microsoft.Commerce.Proxy.AccountService.APIContext();
                input.APIContext.TrackingGuid = request.APIContext.TrackingGuid;
                // put risk properties in both Fraud and property bag because update account can't pass all
                // properties in FraudDetectionContect to risk.
                // properties in PropertBag should go to risk by RiskContext.PartnerInfo.PartnerCustomProperties
                input.APIContext.FraudDetectionContext = ToProxyProperties(request.APIContext.FraudDetectionContext);
                input.APIContext.PropertyBag = ToProxyProperties(request.APIContext.PropertyBag, request.APIContext.FraudDetectionContext);
            }

            if (request.CallerInfo != null)
            {
                input.CallerInfo = new Microsoft.Commerce.Proxy.AccountService.CallerInfo();
                if (request.CallerInfo.Requester != null)
                {
                    input.CallerInfo.Requester = new Microsoft.Commerce.Proxy.AccountService.Identity();
                    input.CallerInfo.Requester.IdentityType = request.CallerInfo.Requester.IdentityType;
                    input.CallerInfo.Requester.IdentityValue = request.CallerInfo.Requester.IdentityValue;

                    switch (request.CallerInfo.Requester.IdentityType)
                    {
                        case "PUID":
                            input.CallerInfo.Requester.IdentityProperty = new Commerce.Proxy.AccountService.Property[1];
                            Commerce.Proxy.AccountService.Property property = new Commerce.Proxy.AccountService.Property();
                            input.CallerInfo.Requester.IdentityProperty[0] = property;
                            property.Name = "PassportMemberName";
                            property.Value = request.CallerInfo.Requester.PassportMemberName;
                            break;
                    }
                }

                // only assign Delegator when it is PUID since backend doesn't support other types for delegator
                if (request.CallerInfo.Delegator != null
                    && !string.IsNullOrEmpty(request.CallerInfo.Delegator.IdentityType)
                    && request.CallerInfo.Delegator.IdentityType.Equals("PUID"))
                {
                    input.CallerInfo.Delegator = new Commerce.Proxy.AccountService.Identity();
                    input.CallerInfo.Delegator.IdentityType = request.CallerInfo.Delegator.IdentityType;
                    input.CallerInfo.Delegator.IdentityValue = request.CallerInfo.Delegator.IdentityValue;
                    switch (request.CallerInfo.Delegator.IdentityType)
                    {
                        case "PUID":
                            input.CallerInfo.Delegator.IdentityProperty = new Commerce.Proxy.AccountService.Property[1];
                            Commerce.Proxy.AccountService.Property property = new Commerce.Proxy.AccountService.Property();
                            input.CallerInfo.Delegator.IdentityProperty[0] = property;
                            property.Name = "PassportMemberName";
                            property.Value = request.CallerInfo.Delegator.PassportMemberName;
                            break;
                    }
                }

                input.CallerInfo.AccountId = request.CallerInfo.AccountId;
            }
            input.OnBehalfOfPartner = request.OnBehalfOfPartner;
            input.AccountInputInfo = BuildAccount(request.Account);
            return input;
        }

        private UpdateAccountResponse ConstructUpdateAccountOutput(UpdateAccountOutput serviceOutput)
        {
            if (serviceOutput == null)
                return null;

            if (serviceOutput.Ack != Microsoft.Commerce.Proxy.AccountService.AckCodeType.Success)
            {
                throw new DataAccessException(
                    ErrorNamespace.Commerce,
                    serviceOutput.Error.ErrorCode,
                    serviceOutput.Error.ErrorShortMessage,
                    serviceOutput.Error.ErrorLongMessage,
                    serviceOutput.Error.ErrorDescription,
                    serviceOutput.Error.Retryable
                    );
            }

            UpdateAccountResponse response = new UpdateAccountResponse();
            response.Account = TransformAccount(serviceOutput.AccountOutputInfo);
            return response;
        }

        private static List<Account> TransformAccounts(AccountInfo[] commerceAccountArray)
        {
            if (commerceAccountArray == null)
                return new List<Account>();

            List<Account> actlist = new List<Account>();
            foreach (var commerceAcct in commerceAccountArray)
            {
                if (commerceAcct.PayinInfo != null)
                {
                    PayinAccount bamPayinAcct = new PayinAccount();
                    bamPayinAcct.AccountID = commerceAcct.AccountID;
                    bamPayinAcct.AccountLevel = commerceAcct.AccountLevel;
                    bamPayinAcct.AccountRole = commerceAcct.AccountRole;
                    bamPayinAcct.FriendlyName = commerceAcct.PayinInfo.FriendlyName;
                    bamPayinAcct.FirstName = commerceAcct.PayinInfo.FirstName;
                    bamPayinAcct.FirstNamePronunciation = commerceAcct.PayinInfo.FirstNamePronunciation;
                    bamPayinAcct.LastName = commerceAcct.PayinInfo.LastName;
                    bamPayinAcct.LastNamePronunciation = commerceAcct.PayinInfo.LastNamePronunciation;
                    bamPayinAcct.Email = commerceAcct.PayinInfo.Email;
                    bamPayinAcct.CompanyName = commerceAcct.PayinInfo.CompanyName;
                    bamPayinAcct.Locale = commerceAcct.PayinInfo.Locale;
                    bamPayinAcct.Currency = commerceAcct.PayinInfo.Currency;
                    bamPayinAcct.CountryCode = commerceAcct.PayinInfo.CountryCode;
                    bamPayinAcct.CreatedDate = DateTime.SpecifyKind(commerceAcct.PayinInfo.CreatedDate, DateTimeKind.Utc);
                    bamPayinAcct.LastUpdatedDate = DateTime.SpecifyKind(commerceAcct.PayinInfo.LastUpdatedDate, DateTimeKind.Utc);
                    bamPayinAcct.AnniversaryDate = commerceAcct.PayinInfo.AnniversaryDate;
                    bamPayinAcct.DefaultAddressID = commerceAcct.PayinInfo.DefaultAddressID;
                    bamPayinAcct.CorporateIdentity = commerceAcct.PayinInfo.CorporateIdentity;
                    bamPayinAcct.CorporateLegalEntity = commerceAcct.PayinInfo.CorporateLegalEntity;
                    bamPayinAcct.CorporateVatId = commerceAcct.PayinInfo.CorporateVatId;
                    bamPayinAcct.CorporateAddress = TransformAddressInfo(commerceAcct.PayinInfo.CorporateAddress);
                    bamPayinAcct.AnniversaryDate = commerceAcct.PayinInfo.AnniversaryDate;

                    if (commerceAcct.PayinInfo.CustomerType != null)
                    {
                        bamPayinAcct.CustomerType = (DataModel.CustomerType)Enum.Parse(typeof(DataModel.CustomerType), commerceAcct.PayinInfo.CustomerType.Value.ToString(), true);

                    }
                    if (commerceAcct.PayinInfo.AddressSet != null)
                    {
                        bamPayinAcct.AddressSet = new List<DataModel.Address>();
                        foreach (var item in commerceAcct.PayinInfo.AddressSet)
                        {
                            if (item == null)
                                continue;
                            bamPayinAcct.AddressSet.Add(
                                new DataModel.Address()
                                {
                                    AddressID = item.AddressID,
                                    City = item.City,
                                    CountryCode = item.CountryCode,
                                    District = item.District,
                                    FriendlyName = item.FriendlyName,
                                    MapAddressResult =
                                        item.MapAddressResult == null ?
                                        null : new DataModel.MapAddressResult()
                                        {
                                            AddressMapAttempted = item.MapAddressResult.AddressMapAttempted,
                                            AddressMapConfidenceScore = item.MapAddressResult.AddressMapConfidenceScore,
                                            AddressMapFailureReason = item.MapAddressResult.AddressMapFailureReason,
                                            AddressMapSucceeded = item.MapAddressResult.AddressMapSucceeded,
                                            ManualAddressSpecified = item.MapAddressResult.ManualAddressSpecified
                                        },
                                    PostalCode = item.PostalCode,
                                    State = item.State,
                                    Street1 = item.Street1,
                                    Street2 = item.Street2,
                                    Street3 = item.Street3,
                                    UnitNumber = item.UnitNumber,
                                    FirstName = item.FirstName,
                                    LastName = item.LastName
                                });
                        }
                    }
                    if (commerceAcct.PayinInfo.PhoneSet != null)
                    {
                        bamPayinAcct.PhoneSet = new List<DataModel.Phone>();
                        foreach (var item in commerceAcct.PayinInfo.PhoneSet)
                        {
                            bamPayinAcct.PhoneSet.Add(new DataModel.Phone()
                            {
                                CountryCode = item.CountryCode,
                                PhoneExtension = item.PhoneExtension,
                                PhoneNumber = item.PhoneNumber,
                                PhonePrefix = item.PhonePrefix,
                                PhoneType = item.PhoneType == null ? DataModel.PhoneType.Primary :
                                (DataModel.PhoneType)Enum.Parse(typeof(DataModel.PhoneType), item.PhoneType.Value.ToString(), true),
                            });
                        }
                    }

                    bamPayinAcct.Status = commerceAcct.PayinInfo.Status == null ? default(DataModel.AccountStatus) :
                        (DataModel.AccountStatus)Enum.Parse(typeof(DataModel.AccountStatus), commerceAcct.PayinInfo.Status.Value.ToString(), true);
                    if (commerceAcct.PayinInfo.Violations != null)
                    {
                        bamPayinAcct.Violations = new List<DataModel.Violation>();
                        foreach (var item in commerceAcct.PayinInfo.Violations)
                        {
                            bamPayinAcct.Violations.Add(new DataModel.Violation()
                            {
                                Name = item.Name,
                                ViolationID = item.ViolationID
                            });
                        }
                    }

                    bamPayinAcct.TaxExemptionSet = TransformTaxExemptionInfo(commerceAcct.PayinInfo.TaxExemptionInfoSet);
                    bamPayinAcct.CustomPropertiesField = TransformCustomProperties(commerceAcct);
                    actlist.Add(bamPayinAcct);
                }

                if (commerceAcct.PayoutInfo != null)
                {
                    PayoutAccount bamPayoutAcct = new PayoutAccount();
                    bamPayoutAcct.AccountID = commerceAcct.AccountID;
                    bamPayoutAcct.AccountLevel = commerceAcct.AccountLevel;
                    bamPayoutAcct.CountryCode = commerceAcct.PayoutInfo.CountryCode;
                    bamPayoutAcct.CreatedDate = commerceAcct.PayoutInfo.CreatedDate;
                    bamPayoutAcct.Currency = commerceAcct.PayoutInfo.Currency;
                    bamPayoutAcct.Email = commerceAcct.PayoutInfo.Email;
                    bamPayoutAcct.FriendlyName = commerceAcct.PayoutInfo.FriendlyName;
                    bamPayoutAcct.LastUpdatedDate = commerceAcct.PayoutInfo.LastUpdatedDate;
                    bamPayoutAcct.Locale = commerceAcct.PayoutInfo.Locale;
                    bamPayoutAcct.Status = commerceAcct.PayoutInfo.Status == null ? default(DataModel.AccountStatus) :
                        (DataModel.AccountStatus)Enum.Parse(typeof(DataModel.AccountStatus), commerceAcct.PayoutInfo.Status.Value.ToString(), true);

                    if (commerceAcct.PayoutInfo.Violations != null)
                    {
                        bamPayoutAcct.Violations = new List<DataModel.Violation>();
                        foreach (var item in commerceAcct.PayoutInfo.Violations)
                        {
                            bamPayoutAcct.Violations.Add(new DataModel.Violation()
                            {
                                Name = item.Name,
                                ViolationID = item.ViolationID
                            });
                        }
                    }

                    actlist.Add(bamPayoutAcct);
                }
            }
            var tmp = from p in actlist
                      where p.AccountLevel == "Primary"
                        || p.AccountLevel == "SecondaryWithSubs"
                        || p.AccountLevel == "Secondary"
                        || p.AccountLevel == "Undetermined"
                        || p.AccountLevel == null
                      select p;
            return tmp.ToList();
        }

        private static PayinPayoutAccount TransformAccount(AccountInfo commerceAccount)
        {
            if (commerceAccount == null)
                return null;
            PayinPayoutAccount account = new PayinPayoutAccount();
            if (commerceAccount.PayinInfo != null)
            {
                account.PayinAccount = TransformPayinAccount(commerceAccount);
                account.AccountID = account.PayinAccount.AccountID;
            }
            if (commerceAccount.PayoutInfo != null)
            {
                account.PayoutAccount = TransformPayoutAccount(commerceAccount);
                account.AccountID = account.PayoutAccount.AccountID;
            }
            return account;
        }

        private static PayinAccount TransformPayinAccount(AccountInfo commerceAccount)
        {
            if (commerceAccount == null)
                return null;
            PayinAccount account = new PayinAccount();
            account.AccountID = commerceAccount.AccountID;
            account.AddressSet = TransformAddressSetInfo(commerceAccount.PayinInfo);
            account.PhoneSet = TransformPhoneSetInfo(commerceAccount.PayinInfo);
            TransformCommonPayinAccountInfo(account, commerceAccount.PayinInfo);

            return account;
        }

        private static PayoutAccount TransformPayoutAccount(AccountInfo commerceAccount)
        {
            if (commerceAccount == null)
                return null;
            PayoutAccount account = new PayoutAccount();
            account.AccountID = commerceAccount.AccountID;
            TransformCommonAccountInfo(account, commerceAccount.PayoutInfo);

            return account;
        }

        private static AccountInfo BuildAccount(PayinPayoutAccount account)
        {
            if (account == null)
                return null;
            AccountInfo accountInfo = new AccountInfo();
            if (account.PayinAccount != null)
            {
                accountInfo.PayinInfo = BuildAccountPayinInfo(account.PayinAccount);
                accountInfo.CustomProperties = BuildCustomProperties(account.PayinAccount);
            }
            if (account.PayoutAccount != null)
            {
                accountInfo.PayoutInfo = BuildAccountPayoutInfo(account.PayoutAccount);
            }

            return accountInfo;
        }

        private static ProxyProperty[] BuildCustomProperties(PayinAccount account)
        {
            if (account == null || account.CustomPropertiesField == null)
            {
                return null;
            }

            DataModel.Property[] properties = account.CustomPropertiesField;
            int count = properties.Length;

            if (count <= 0)
            {
                return null;
            }

            ProxyProperty[] CustomProperties = new ProxyProperty[count];
            for (int i = 0; i < count; i++)
            {
                DataModel.Property p = properties[i];
                CustomProperties[i] = new ProxyProperty();
                CustomProperties[i].Namespace = p.Namespace;
                CustomProperties[i].Name = p.Name;
                CustomProperties[i].Value = p.Value;
            }

            return CustomProperties;
        }

        private static DataModel.Property[] TransformCustomProperties(AccountInfo account)
        {
            if (account == null || account.CustomProperties == null)
            {
                return null;
            }

            ProxyProperty[] properties = account.CustomProperties;
            int count = properties.Length;

            if (count <= 0)
            {
                return null;
            }

            DataModel.Property[] customProperties = new DataModel.Property[count];
            for (int i = 0; i < count; i++)
            {
                ProxyProperty p = properties[i];
                customProperties[i] = new DataModel.Property();
                customProperties[i].Namespace = p.Namespace;
                customProperties[i].Name = p.Name;
                customProperties[i].Value = p.Value;
            }

            return customProperties;
        }

        private static PayinAccountInfo BuildAccountPayinInfo(PayinAccount account)
        {
            if (account == null)
                return null;
            PayinAccountInfo payinInfo = new PayinAccountInfo();
            payinInfo.AddressSet = BuildAccountAddressSetInfo(account);
            payinInfo.PhoneSet = BuildAccountPhoneSetInfo(account);
            payinInfo.CountryCode = account.CountryCode;
            payinInfo.Currency = account.Currency;
            payinInfo.Email = account.Email;
            payinInfo.Locale = account.Locale;
            payinInfo.FirstName = account.FirstName;
            payinInfo.FirstNamePronunciation = account.FirstNamePronunciation;
            payinInfo.LastName = account.LastName;
            payinInfo.LastNamePronunciation = account.LastNamePronunciation;
            payinInfo.CompanyName = account.CompanyName;
            payinInfo.CompanyNamePronunciation = account.CompanyNamePronunciation;
            payinInfo.FriendlyName = account.FriendlyName;

            if (account.CustomerType.HasValue)
            {
                payinInfo.CustomerType = (Microsoft.Commerce.Proxy.AccountService.CustomerType)Enum.Parse(typeof(Microsoft.Commerce.Proxy.AccountService.CustomerType), account.CustomerType.Value.ToString(), true);
            }

            switch (payinInfo.CustomerType)
            {
                case Microsoft.Commerce.Proxy.AccountService.CustomerType.Personal:
                    payinInfo.TaxExemptionInfoSet = BuildTaxExemptionInfo(account);
                    break;
                case Microsoft.Commerce.Proxy.AccountService.CustomerType.Business:
                case Microsoft.Commerce.Proxy.AccountService.CustomerType.Corporate:
                    payinInfo.TaxExemptionInfoSet = BuildTaxExemptionInfo(account);
                    break;
            }

            return payinInfo;
        }

        private static PayoutAccountInfo BuildAccountPayoutInfo(PayoutAccount account)
        {
            if (account == null)
            {
                return null;
            }
            PayoutAccountInfo payoutInfo = new PayoutAccountInfo();
            payoutInfo.CountryCode = account.CountryCode;
            payoutInfo.Currency = account.Currency;
            payoutInfo.Email = account.Email;
            payoutInfo.Locale = account.Locale;

            return payoutInfo;
        }

        #region common accountInfo

        private static Microsoft.Commerce.Proxy.AccountService.TaxExemptionType? BuildTaxExemptionType(DataModel.TaxExemptionType? type)
        {
            if (!type.HasValue)
                return null;

            switch (type)
            {
                case DataModel.TaxExemptionType.CanadianFederalExempt:
                    return Microsoft.Commerce.Proxy.AccountService.TaxExemptionType.CanadianFederalExempt;
                case DataModel.TaxExemptionType.CanadianProvinceExempt:
                    return Microsoft.Commerce.Proxy.AccountService.TaxExemptionType.CanadianProvinceExempt;
                case DataModel.TaxExemptionType.USExempt:
                    return Microsoft.Commerce.Proxy.AccountService.TaxExemptionType.USExempt;
                case DataModel.TaxExemptionType.VATID:
                    return Microsoft.Commerce.Proxy.AccountService.TaxExemptionType.VATID;
                case DataModel.TaxExemptionType.BrazilCPFID:
                    return Microsoft.Commerce.Proxy.AccountService.TaxExemptionType.BrazilCPFID;
                case DataModel.TaxExemptionType.BrazilCNPJID:
                    return Microsoft.Commerce.Proxy.AccountService.TaxExemptionType.BrazilCNPJID;
                case DataModel.TaxExemptionType.BrazilCCMID:
                    return Microsoft.Commerce.Proxy.AccountService.TaxExemptionType.BrazilCCMID;
                default:
                    return null;
            }
        }

        private static TaxExemptionInfo[] BuildTaxExemptionInfo(PayinAccount account)
        {
            if (account.TaxExemptionSet != null)
            {
                TaxExemptionInfo[] taxExemptionInfos = new TaxExemptionInfo[account.TaxExemptionSet.Count];

                for (int i = 0; i < account.TaxExemptionSet.Count; i++)
                {
                    TaxExemption te = account.TaxExemptionSet[i];
                    TaxExemptionInfo tei = new TaxExemptionInfo()
                    {
                        Status = BuildTaxExemptionStatus(te.Status),
                        CertificateNumber = te.CertificateNumber,
                        Type = BuildTaxExemptionType(te.TaxExemptionType),
                        ExpDate = DateTime.MaxValue.Date,
                    };

                    taxExemptionInfos[i] = tei;
                }

                return taxExemptionInfos;
            }

            return null;
        }

        private static Commerce.Proxy.AccountService.TaxExemptionStatus? BuildTaxExemptionStatus(DataModel.TaxExemptionStatus? status)
        {
            if (!status.HasValue) return null;
            switch (status)
            {
                case DataModel.TaxExemptionStatus.Expired:
                    return Microsoft.Commerce.Proxy.AccountService.TaxExemptionStatus.Expired;
                case DataModel.TaxExemptionStatus.Invalid:
                    return Microsoft.Commerce.Proxy.AccountService.TaxExemptionStatus.Invalid;
                case DataModel.TaxExemptionStatus.PastDue:
                    return Microsoft.Commerce.Proxy.AccountService.TaxExemptionStatus.PastDue;
                case DataModel.TaxExemptionStatus.Pending:
                    return Microsoft.Commerce.Proxy.AccountService.TaxExemptionStatus.Pending;
                case DataModel.TaxExemptionStatus.Valid:
                    return Microsoft.Commerce.Proxy.AccountService.TaxExemptionStatus.Valid;
                default:
                    return null;
            }
        }

        private static List<TaxExemption> TransformTaxExemptionInfo(TaxExemptionInfo[] taxExemptionInfoSet)
        {
            if (taxExemptionInfoSet == null) { return null; }
            if (taxExemptionInfoSet.Length == 0) { return new List<TaxExemption>(); }
            List<TaxExemption> list = new List<TaxExemption>();
            foreach (var item in taxExemptionInfoSet)
            {
                TaxExemption txe = new TaxExemption();
                txe.CertificateNumber = item.CertificateNumber;
                txe.DateAdded = item.DateAdded;
                txe.DateReceived = item.DateReceived;
                txe.ExpDate = item.ExpDate;
                txe.Status = TransformTaxExemptionStatus(item.Status);
                txe.TaxExemptionType = TransformTaxExemptionType(item.Type);
                list.Add(txe);
            }
            return list;
        }

        private static DataModel.TaxExemptionStatus? TransformTaxExemptionStatus(
            Microsoft.Commerce.Proxy.AccountService.TaxExemptionStatus? status
            )
        {
            if (!status.HasValue) return null;
            switch (status)
            {
                case Microsoft.Commerce.Proxy.AccountService.TaxExemptionStatus.Expired:
                    return DataModel.TaxExemptionStatus.Expired;
                case Microsoft.Commerce.Proxy.AccountService.TaxExemptionStatus.Invalid:
                    return DataModel.TaxExemptionStatus.Invalid;
                case Microsoft.Commerce.Proxy.AccountService.TaxExemptionStatus.PastDue:
                    return DataModel.TaxExemptionStatus.PastDue;
                case Microsoft.Commerce.Proxy.AccountService.TaxExemptionStatus.Pending:
                    return DataModel.TaxExemptionStatus.Pending;
                case Microsoft.Commerce.Proxy.AccountService.TaxExemptionStatus.Valid:
                    return DataModel.TaxExemptionStatus.Valid;
                default:
                    return null;
            }

        }

        private static DataModel.TaxExemptionType? TransformTaxExemptionType(
            Microsoft.Commerce.Proxy.AccountService.TaxExemptionType? type
            )
        {
            if (!type.HasValue) return null;
            switch (type)
            {
                case Microsoft.Commerce.Proxy.AccountService.TaxExemptionType.CanadianFederalExempt:
                    return DataModel.TaxExemptionType.CanadianFederalExempt;
                case Microsoft.Commerce.Proxy.AccountService.TaxExemptionType.CanadianProvinceExempt:
                    return DataModel.TaxExemptionType.CanadianProvinceExempt;
                case Microsoft.Commerce.Proxy.AccountService.TaxExemptionType.USExempt:
                    return DataModel.TaxExemptionType.USExempt;
                case Microsoft.Commerce.Proxy.AccountService.TaxExemptionType.VATID:
                    return DataModel.TaxExemptionType.VATID;
                case Microsoft.Commerce.Proxy.AccountService.TaxExemptionType.BrazilCPFID:
                    return DataModel.TaxExemptionType.BrazilCPFID;
                case Microsoft.Commerce.Proxy.AccountService.TaxExemptionType.BrazilCNPJID:
                    return DataModel.TaxExemptionType.BrazilCNPJID;
                case Microsoft.Commerce.Proxy.AccountService.TaxExemptionType.BrazilCCMID:
                    return DataModel.TaxExemptionType.BrazilCCMID;
                default:
                    return null;
            }
        }

        private static void TransformCommonPayinAccountInfo(PayinAccount account, PayinAccountInfo commercePayinAccount)
        {
            account.CountryCode = commercePayinAccount.CountryCode;
            account.Currency = commercePayinAccount.Currency;
            account.Email = commercePayinAccount.Email;
            account.Locale = commercePayinAccount.Locale;
            account.Status = commercePayinAccount.Status == null ? default(DataModel.AccountStatus) :
                        (DataModel.AccountStatus)Enum.Parse(typeof(DataModel.AccountStatus), commercePayinAccount.Status.Value.ToString(), true);

            account.FirstName = commercePayinAccount.FirstName;
            account.FirstNamePronunciation = commercePayinAccount.FirstNamePronunciation;
            account.LastName = commercePayinAccount.LastName;
            account.LastNamePronunciation = commercePayinAccount.LastNamePronunciation;
            account.CompanyName = commercePayinAccount.CompanyName;
            account.CompanyNamePronunciation = commercePayinAccount.CompanyNamePronunciation;
            account.FriendlyName = commercePayinAccount.FriendlyName;

            account.CustomerType = (DataModel.CustomerType)Enum.Parse(typeof(DataModel.CustomerType), commercePayinAccount.CustomerType.Value.ToString(), true);

            switch (commercePayinAccount.CustomerType)
            {
                case Microsoft.Commerce.Proxy.AccountService.CustomerType.Personal:
                    break;
                case Microsoft.Commerce.Proxy.AccountService.CustomerType.Business:
                case Microsoft.Commerce.Proxy.AccountService.CustomerType.Corporate:
                    account.TaxExemptionSet = TransformTaxExemptionInfo(commercePayinAccount.TaxExemptionInfoSet);
                    account.CorporateVatId = commercePayinAccount.CorporateVatId;
                    break;
            }
        }



        private static void TransformCommonAccountInfo(PayoutAccount account, PayoutAccountInfo payoutAccountInfo)
        {
            account.CountryCode = payoutAccountInfo.CountryCode;
            account.Currency = payoutAccountInfo.Currency;
            account.Email = payoutAccountInfo.Email;
            account.Locale = payoutAccountInfo.Locale;
            account.Status = payoutAccountInfo.Status == null ? default(DataModel.AccountStatus) :
                        (DataModel.AccountStatus)Enum.Parse(typeof(DataModel.AccountStatus), payoutAccountInfo.Status.Value.ToString(), true);
        }
        #endregion

        #region Account Phone Convert
        private static Microsoft.Commerce.Proxy.AccountService.Phone[] BuildAccountPhoneSetInfo(PayinAccount account)
        {
            if (account.PhoneSet == null)
            {
                return null;
            }

            Microsoft.Commerce.Proxy.AccountService.Phone[] phoneSet = new Microsoft.Commerce.Proxy.AccountService.Phone[1];
            if (account.PhoneSet.Count > 0)
            {
                phoneSet[0] = BuildAccountPhoneInfo(account.PhoneSet[0]);
            }

            return phoneSet;
        }

        private static Microsoft.Commerce.Proxy.AccountService.Phone BuildAccountPhoneInfo(DataModel.Phone commercePhone)
        {
            Microsoft.Commerce.Proxy.AccountService.Phone phone = new Microsoft.Commerce.Proxy.AccountService.Phone();
            phone.CountryCode = commercePhone.CountryCode;
            phone.PhoneType = commercePhone.PhoneType == null ? Commerce.Proxy.AccountService.PhoneType.Primary :
                            (Commerce.Proxy.AccountService.PhoneType)Enum.Parse(typeof(Commerce.Proxy.AccountService.PhoneType), commercePhone.PhoneType.Value.ToString(), true);
            phone.PhonePrefix = commercePhone.PhonePrefix;
            if (string.IsNullOrEmpty(phone.PhonePrefix))
            {
                phone.PhonePrefix = "-";
            }
            phone.PhoneNumber = commercePhone.PhoneNumber;
            phone.PhoneExtension = commercePhone.PhoneExtension;

            return phone;
        }

        private static List<DataModel.Phone> TransformPhoneSetInfo(PayinAccountInfo commercePayinAccount)
        {
            if (commercePayinAccount.PhoneSet == null)
            {
                return null;
            }

            List<DataModel.Phone> phoneSet = new List<DataModel.Phone>();
            for (int i = 0; i < commercePayinAccount.PhoneSet.Length; i++)
            {
                phoneSet.Add(TransformPhoneInfo(commercePayinAccount.PhoneSet[i]));
            }

            return phoneSet;
        }

        private static DataModel.Phone TransformPhoneInfo(Microsoft.Commerce.Proxy.AccountService.Phone commercePhone)
        {
            DataModel.Phone phone = new DataModel.Phone();
            phone.CountryCode = commercePhone.CountryCode;
            phone.PhoneType = commercePhone.PhoneType == null ? DataModel.PhoneType.Primary :
                            (DataModel.PhoneType)Enum.Parse(typeof(DataModel.PhoneType), commercePhone.PhoneType.Value.ToString(), true);
            phone.PhonePrefix = commercePhone.PhonePrefix ?? string.Empty;
            phone.PhoneNumber = commercePhone.PhoneNumber;
            phone.PhoneExtension = commercePhone.PhoneExtension;

            return phone;
        }

        #endregion

        #region Account Address Convert
        private static Microsoft.Commerce.Proxy.AccountService.Address[] BuildAccountAddressSetInfo(PayinAccount account)
        {
            if (account.AddressSet == null)
            {
                return null;
            }

            Microsoft.Commerce.Proxy.AccountService.Address[] addressSet = new Microsoft.Commerce.Proxy.AccountService.Address[1];
            if (account.AddressSet.Count > 0)
            {
                addressSet[0] = BuildAccountAddressInfo(account.AddressSet[0]);
            }

            return addressSet;
        }

        private static Microsoft.Commerce.Proxy.AccountService.Address BuildAccountAddressInfo(DataModel.Address commerceAddress)
        {
            Microsoft.Commerce.Proxy.AccountService.Address address = new Microsoft.Commerce.Proxy.AccountService.Address();
            address.AddressID = commerceAddress.AddressID;
            address.City = commerceAddress.City;
            address.State = commerceAddress.State;
            address.CountryCode = commerceAddress.CountryCode;
            address.District = commerceAddress.District;
            address.FriendlyName = commerceAddress.FriendlyName;
            address.PostalCode = commerceAddress.PostalCode;
            address.Street1 = commerceAddress.Street1;
            address.Street2 = commerceAddress.Street2;
            address.Street3 = commerceAddress.Street3;

            return address;
        }

        private static List<DataModel.Address> TransformAddressSetInfo(PayinAccountInfo commercePayinAccount)
        {
            if (commercePayinAccount.AddressSet == null)
            {
                return null;
            }

            List<DataModel.Address> addressSet = new List<DataModel.Address>();
            for (int i = 0; i < commercePayinAccount.AddressSet.Length; i++)
            {
                addressSet.Add(TransformAddressInfo(commercePayinAccount.AddressSet[i]));
            }

            return addressSet;
        }

        private static DataModel.Address TransformAddressInfo(Microsoft.Commerce.Proxy.AccountService.Address commerceAddress)
        {
            if (commerceAddress == null)
                return null;

            DataModel.Address address = new DataModel.Address();
            address.AddressID = commerceAddress.AddressID;
            address.City = commerceAddress.City;
            address.State = commerceAddress.State;
            address.CountryCode = commerceAddress.CountryCode;
            address.District = commerceAddress.District;
            address.FriendlyName = commerceAddress.FriendlyName;
            address.PostalCode = commerceAddress.PostalCode;
            address.Street1 = commerceAddress.Street1;
            address.Street2 = commerceAddress.Street2;
            address.Street3 = commerceAddress.Street3;

            return address;
        }
        #endregion


        public GetAccountIdFromPaymentInstrumentInfoResponse GetAccountIdFromPaymentInstrumentInfo(GetAccountIdFromPaymentInstrumentInfoRequest request)
        {
            throw new NotImplementedException();
        }
    }
}