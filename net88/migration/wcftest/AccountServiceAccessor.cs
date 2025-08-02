// <copyright file="AccountServiceAccessor.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Commerce.Payments.Common;
    using Microsoft.Commerce.Payments.Common.Tracing;
    using Microsoft.Commerce.Payments.PimsModel.V4;
    using Microsoft.Commerce.Payments.PXCommon;
    using Microsoft.Commerce.Payments.PXService.Accessors.AccountService.DataModel;
    using Microsoft.Commerce.Payments.PXService.AddressEnrichmentService.V7;
    using Microsoft.Commerce.Payments.PXService.Model.AccountService;
    using Microsoft.Commerce.Payments.PXService.V7;
    using Model.AccountService.AddressValidation;
    using Newtonsoft.Json;
    using static Microsoft.Commerce.Payments.PXService.GlobalConstants;

    public class AccountServiceAccessor : IAccountServiceAccessor
    {
        private const string AccountServiceName = "AccountService";

        private PXTracingHttpClient accountServiceHttpClient;

        private string serviceBaseUrl;
        private string emulatorBaseUrl;

        public AccountServiceAccessor(
            string serviceBaseUrl,
            string emulatorBaseUrl)
        {
            this.serviceBaseUrl = serviceBaseUrl;
            this.emulatorBaseUrl = emulatorBaseUrl;

            var defaultHeaders = new Dictionary<string, string>
            {
                { "Accept", PaymentConstants.HttpMimeTypes.JsonContentType },
                { PaymentConstants.HttpHeaders.Connection, PaymentConstants.HttpHeaders.KeepAlive },
                { PaymentConstants.HttpHeaders.KeepAlive, string.Format(PaymentConstants.HttpHeaders.KeepAliveParameter, 60) }
            };

            this.accountServiceHttpClient = new PXTracingHttpClient(AccountService.V7.Constants.ServiceNames.AccountService, defaultHeaders);
        }

        private string BaseUrl => string.IsNullOrWhiteSpace(this.emulatorBaseUrl) ? this.serviceBaseUrl : this.emulatorBaseUrl;

        public async Task<AddressInfoV3> PatchAddress(string accountId, string addressId, AddressInfoV3 address, string eTag, EventTraceActivity traceActivityId)
        {
            string requestUrl = string.Format(V7.Constants.UriTemplate.PatchAddressByAddressId, accountId, addressId);
            if (string.IsNullOrEmpty(eTag))
            {
                var fullAddress = await this.GetAddress<AddressInfoV3>(accountId, addressId, GlobalConstants.AccountServiceApiVersion.V3, traceActivityId);
                eTag = fullAddress.Etag;
            }

            return await this.SendRequest<AddressInfoV3>(
                    requestUrl,
                    GlobalConstants.AccountServiceApiVersion.V3,
                    address,
                    "PatchAddress",
                    traceActivityId,
                    GlobalConstants.HTTPVerbs.PATCH,
                    eTag);
        }

        public async Task<T> PostAddress<T>(string accountId, T address, string apiVersion, EventTraceActivity traceActivityId)
        {
            return await this.PostAddress<T>(accountId, address, apiVersion, null, traceActivityId);
        }

        public async Task<T> PostAddress<T>(string accountId, T address, string apiVersion, int? syncToLegacyCode, EventTraceActivity traceActivityId)
        {
            string requestUrl = string.Format(V7.Constants.UriTemplate.PostAddressForAccountId, accountId);

            if (syncToLegacyCode.HasValue)
            {
                requestUrl = string.Format(V7.Constants.UriTemplate.JarvisPostAddressSyncLegacy3, accountId, syncToLegacyCode.Value);
            }

            return await this.SendPostRequest<T>(
                    requestUrl,
                    apiVersion,
                    address,
                    "PostAddress",
                    traceActivityId,
                    null,
                    HandlePostAddressValidationError);
        }

        /// <summary>
        /// Gets the customer details by customer id from Jarvis
        /// </summary>
        /// <param name="customerId">A given customer id for whom the customer details needs to be fetched</param>
        /// <param name="traceActivityId">EventTraceActivity object</param>
        /// <returns>Customer details</returns>
        public async Task<CustomerInfo> GetCustomers(string customerId, EventTraceActivity traceActivityId)
        {
            // Call Jarvis's Get-By-Identity API to get the employee customer id
            string requestUrl = string.Format(V7.Constants.UriTemplate.GetCustomerById, customerId);
            CustomerInfo customerDetails = await this.SendGetRequest<CustomerInfo>(
                requestUrl,
                GlobalConstants.AccountServiceApiVersion.V3,
                "GetCustomers",
                traceActivityId);
            return customerDetails;
        }

        public async Task<AccountProfile> GetProfile(string accountId, string type, EventTraceActivity traceActivityId)
        {
            string requestUrl = string.Format(V7.Constants.UriTemplate.GetProfilesByAccountId, accountId, type);
            AccountProfile profile = null;

            try
            {
                AccountProfiles profiles = await this.SendGetRequest<AccountProfiles>(
                    requestUrl,
                    GlobalConstants.AccountServiceApiVersion.V2,
                    "GetProfiles",
                    traceActivityId);
                if (profiles != null && profiles.UserProfiles != null)
                {
                    return profiles.UserProfiles.FirstOrDefault<AccountProfile>();
                }
            }
            catch (FailedOperationException ex)
            {
                if (!ex.Message.Contains(StatusCodes.Status404NotFound.ToString()))
                {
                    throw;
                }
            }

            return profile;
        }

        public async Task<AccountProfileV3> GetProfileV3(string accountId, string type, EventTraceActivity traceActivityId)
        {
            string requestUrl = string.Format(V7.Constants.UriTemplate.GetProfilesByAccountId, accountId, type);
            AccountProfileV3 profile = null;

            if (string.Equals(type, GlobalConstants.ProfileTypes.Employee, StringComparison.InvariantCultureIgnoreCase))
            {
                profile = await this.GetProfileV3ByType<AccountEmployeeProfileV3>(requestUrl, traceActivityId);
            }
            else if (string.Equals(type, GlobalConstants.ProfileTypes.Organization, StringComparison.InvariantCultureIgnoreCase))
            {
                profile = await this.GetProfileV3ByType<AccountOrganizationProfileV3>(requestUrl, traceActivityId);
            }
            else if (string.Equals(type, GlobalConstants.ProfileTypes.Consumer, StringComparison.InvariantCultureIgnoreCase))
            {
                profile = await this.GetProfileV3ByType<AccountConsumerProfileV3>(requestUrl, traceActivityId);
            }

            return profile;
        }

        public async Task UpdateProfile(string accountId, AccountProfile profile, EventTraceActivity traceActivityId)
        {
            string requestUrl = string.Format(V7.Constants.UriTemplate.UpdateProfilesById, accountId, profile.Id);
            await this.SendPostRequest<AccountProfiles>(
                requestUrl,
                GlobalConstants.AccountServiceApiVersion.V2,
                profile,
                "UpdateProfile",
                traceActivityId);
        }

        public async Task UpdateProfileV3(string accountId, AccountProfileV3 profile, string type, EventTraceActivity traceActivityId, List<string> exposedFlightFeatures, bool syncLegacyAddress = true, bool useJarvisPatchForConsumerProfile = false)
        {   
            string requestUrl = string.Format(V7.Constants.UriTemplate.UpdateProfilesById, accountId, profile.Id);

            if (!syncLegacyAddress && exposedFlightFeatures.Contains(Flighting.Features.PXJarvisProfileCallSyncLegacyAddressFalse))
            {
                requestUrl += "?syncLegacyAddress=false";
            }

            if (string.Equals(type, GlobalConstants.ProfileTypes.Employee, StringComparison.InvariantCultureIgnoreCase))
            {
                await this.SendPostRequest<AccountProfilesV3<AccountEmployeeProfileV3>>(
                    requestUrl,
                    GlobalConstants.AccountServiceApiVersion.V3,
                    profile,
                    "UpdateProfile",
                    traceActivityId,
                    profile.Etag);
            }
            else if (string.Equals(type, GlobalConstants.ProfileTypes.Organization, StringComparison.InvariantCultureIgnoreCase))
            {
                await this.SendPostRequest<AccountProfilesV3<AccountOrganizationProfileV3>>(
                    requestUrl,
                    GlobalConstants.AccountServiceApiVersion.V3,
                    profile,
                    "UpdateProfile",
                    traceActivityId,
                    profile.Etag);
            }
            else if (string.Equals(type, GlobalConstants.ProfileTypes.Consumer, StringComparison.InvariantCultureIgnoreCase))
            {
                // Check is Patch Profile
                var isPatchProfile = false;
                if (exposedFlightFeatures.Contains(Flighting.Features.UseJarvisPatchForConsumerProfile)
                    || useJarvisPatchForConsumerProfile)
                {
                    isPatchProfile = true;

                    // In Patch profile either DefaultAddressId or Default_address needed.
                    if (!string.IsNullOrEmpty(profile.DefaultAddressId))
                    {
                        profile.Links = null;
                    }
                }

                await this.SendRequest<AccountProfilesV3<AccountConsumerProfileV3>>(
                    requestUrl,
                    GlobalConstants.AccountServiceApiVersion.V3,
                    profile,
                    isPatchProfile ? "PatchProfile" : "PutProfile",
                    traceActivityId,
                    isPatchProfile ? GlobalConstants.HTTPVerbs.PATCH : GlobalConstants.HTTPVerbs.PUT,
                    profile.Etag);
            }
        }

        public async Task<T> GetAddress<T>(string accountId, string addressId, string apiVersion, EventTraceActivity traceActivityId)
        {
            string requestUrl = string.Format(V7.Constants.UriTemplate.GetAddressByAddressId, accountId, addressId);
            T address = await this.SendGetRequest<T>(
                requestUrl,
                apiVersion,
                "GetAddressById",
                traceActivityId);
            return address;
        }

        public async Task<T> GetAddressesByCountry<T>(string accountId, string country, string apiVersion, EventTraceActivity traceActivityId)
        {
            string requestUrl = string.Format(V7.Constants.UriTemplate.GetAddressesByCountry, accountId, country);
            T addresses = await this.SendGetRequest<T>(
                requestUrl,
                apiVersion,
                "GetAddressByCountry",
                traceActivityId);
            return addresses;
        }

        /// <summary>
        /// Gets the Legal Entity (LE) profile from Jarvis
        /// </summary>
        /// <param name="tenantId">A given tenant id for whom the profile needs to be fetched</param>
        /// <param name="traceActivityId">EventTraceActivity object</param>
        /// <returns>The LE profile - Type will be updated in future implementations</returns>
        public async Task<AccountLegalProfileV3> GetLegalEntityProfile(string tenantId, EventTraceActivity traceActivityId)
        {
            try
            {
                // Call Jarvis's Get-By-Identity API to get the tenants customer id
                string requestUrl = string.Format(V7.Constants.UriTemplate.GetTenantCustomerByIdentity, tenantId);
                CommerceAccountCustomer commerceAccountCustomer = await this.SendGetRequest<CommerceAccountCustomer>(requestUrl, GlobalConstants.AccountServiceApiVersion.V3, "GetTenantCustomerByIdentity", traceActivityId);

                // Use the organization customer id to get the company customer id 
                string organizationCustomerId = commerceAccountCustomer.Id.ToString();
                var path = string.Format("/{0}/relationships/get-reverse-relationships?type=Is-Company-For", organizationCustomerId);
                GetRelationshipsResponse getRelationshipsResponse = await this.SendGetRequest<GetRelationshipsResponse>(path, GlobalConstants.AccountServiceApiVersion.V3, "GetRelationships", new EventTraceActivity());
                string companyCustomerId = string.Empty;
                if (getRelationshipsResponse != null && getRelationshipsResponse.Items != null && getRelationshipsResponse.Items.Any())
                {
                    companyCustomerId = getRelationshipsResponse.Items[0].AccountId;
                }

                // Get the legal entity profile using the company customer id and GetProfiles API
                var getProfilePath = string.Format("/{0}/profiles?type=legal_entity", companyCustomerId);
                GetProfilesResponse getProfileResponse = await this.SendGetRequest<GetProfilesResponse>(getProfilePath, GlobalConstants.AccountServiceApiVersion.V3, "GetProfiles", new EventTraceActivity());
                if (getProfileResponse != null && getProfileResponse.Items != null && getProfileResponse.Items.Any())
                {
                    // Only 1 profile of a particular type is allowed, hence there will be only 1 legal entity profile if it exists
                    return getProfileResponse.Items.First();
                }

                return null;
            }
            catch (Exception ex)
            {
                string message = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                throw TraceCore.TraceException(traceActivityId, new FailedOperationException("Failed to GET the Legal Entity profile" + message));
            }
        }

        /// <summary>
        /// Gets the employee profile customer id from Jarvis
        /// </summary>
        /// <param name="tenantId">A given organization id for whom the account info needs to be fetched</param>
        /// <param name="organizationId">A given tenant id for whom the account info needs to be fetched</param>
        /// <param name="traceActivityId">EventTraceActivity object</param>
        /// <returns>The employee profile customer id</returns>
        public async Task<string> GetEmployeeProfileAccountId(string tenantId, string organizationId, EventTraceActivity traceActivityId)
        {
            // Call Jarvis's Get-By-Identity API to get the employee customer id
            string requestUrl = string.Format(V7.Constants.UriTemplate.GetEmployeeCustomerByIdentity, tenantId, organizationId);
            CommerceAccountCustomer commerceAccountCustomer = await this.SendGetRequest<CommerceAccountCustomer>(
                requestUrl,
                GlobalConstants.AccountServiceApiVersion.V3,
                "GetEmployeeCustomerByIdentity",
                traceActivityId);
            return commerceAccountCustomer.Id.ToString();
        }

        public async Task<object> LegacyValidateAddress(object address, EventTraceActivity traceActivityId)
        {
            return await this.SendPostRequest<object>(
                    V7.Constants.UriTemplate.LegacyAddressValidation,
                    GlobalConstants.AccountServiceApiVersion.V3,
                    address,
                    "LegacyValidation",
                    traceActivityId,
                    null,
                    HandleLegacyAddressValidationError);
        }

        public async Task<T> ModernValidateAddress<T>(object address, EventTraceActivity traceActivityId, bool regionIsoEnabled = false)
        {
            return await this.SendPostRequest<T>(
                    V7.Constants.UriTemplate.ModernAddressValidation,
                    GlobalConstants.AccountServiceApiVersion.V3,
                    address,
                     "ModernValidation",
                    traceActivityId,
                    regionIsoEnabled: regionIsoEnabled);
        }

        public async Task<LegacyBillableAccount> GetOrCreateLegacyBillableAccount(string accountId, string country, EventTraceActivity traceActivityId)
        {
            string requestUrl = string.Format(V7.Constants.UriTemplate.JarvisGetOrCreateLegacyBillableAccount, accountId, country);
            return await this.SendPostRequest<LegacyBillableAccount>(
                requestUrl,
                GlobalConstants.AccountServiceApiVersion.V3,
                null,
                "GetOrCreateLegacyBillableAccount",
                traceActivityId);
        }

        private static Task HandleLegacyAddressValidationError(PXHttpResponse response, EventTraceActivity traceActivityId)
        {
            string responseMessage = response.Content;
            ServiceErrorResponse error = null;
            try
            {
                LegacyAddressValidationErrorResponse errorDetails = JsonConvert.DeserializeObject<LegacyAddressValidationErrorResponse>(responseMessage);
                var innerError = new ServiceErrorResponse(errorDetails.Code, errorDetails.Reason, AccountServiceName);
                error = new ServiceErrorResponse(traceActivityId.ActivityId.ToString(), GlobalConstants.ServiceName, innerError);
            }
            catch
            {
                throw TraceCore.TraceException(traceActivityId, new FailedOperationException($"Failed to deserialize error response from Account service: {responseMessage}"));
            }

            throw TraceCore.TraceException(traceActivityId, new ServiceErrorResponseException() { Error = error, Response = response });
        }

        private static Task HandlePostAddressValidationError(PXHttpResponse response, EventTraceActivity traceActivityId)
        {
            string responseMessage = response.Content;
            ServiceErrorResponse error = null;
            try
            {
                PostAddressValidationErrorResponse errorDetails = JsonConvert.DeserializeObject<PostAddressValidationErrorResponse>(responseMessage);
                errorDetails = TransformJarvisErrorCodeToPIDLServerErrorCode(errorDetails);

                if (errorDetails != null && errorDetails.Parameters != null && !string.IsNullOrEmpty(errorDetails.Parameters.Details))
                {
                    errorDetails.Message = $"{errorDetails.Message}{errorDetails.Parameters.Details}";
                }

                var innerError = new ServiceErrorResponse(errorDetails.Error_code, errorDetails.Message, AccountServiceName);
                error = new ServiceErrorResponse(traceActivityId.ActivityId.ToString(), GlobalConstants.ServiceName, innerError);
            }
            catch
            {
                throw TraceCore.TraceException(traceActivityId, new FailedOperationException($"Failed to deserialize error response from Account service: {responseMessage}"));
            }

            throw TraceCore.TraceException(traceActivityId, new ServiceErrorResponseException() { Error = error, Response = response });
        }

        private static PostAddressValidationErrorResponse TransformJarvisErrorCodeToPIDLServerErrorCode(PostAddressValidationErrorResponse postAddressValidationErrorResponse)
        {
            switch (postAddressValidationErrorResponse.Parameters.Property_name)
            {
                case AddressErrorTargets.AddressLine1:
                    postAddressValidationErrorResponse.Error_code = PostAddressErrorCodes.InvalidStreet;
                    break;

                case AddressErrorTargets.AddressLine2:
                    postAddressValidationErrorResponse.Error_code = PostAddressErrorCodes.InvalidStreet;
                    break;

                case AddressErrorTargets.AddressLine3:
                    postAddressValidationErrorResponse.Error_code = PostAddressErrorCodes.InvalidStreet;
                    break;

                case AddressErrorTargets.City:
                    postAddressValidationErrorResponse.Error_code = PostAddressErrorCodes.InvalidCity;
                    break;

                case AddressErrorTargets.PostalCode:
                    postAddressValidationErrorResponse.Error_code = PostAddressErrorCodes.InvalidPostalCode;
                    break;

                case AddressErrorTargets.State:
                    postAddressValidationErrorResponse.Error_code = PostAddressErrorCodes.InvalidRegion;
                    break;

                case PostAddressErrorCodes.JarvisAddressFieldCombination:
                    postAddressValidationErrorResponse.Error_code = PostAddressErrorCodes.InvalidAddressFieldsCombination;
                    break;

                default:
                    postAddressValidationErrorResponse.Error_code = PostAddressErrorCodes.InvalidParameter;
                    break;
            }

            return postAddressValidationErrorResponse;
        }

        private async Task<T> SendGetRequest<T>(string requestUrl, string apiVersion, string actionName, EventTraceActivity traceActivityId)
        {
            string fullRequestUrl = string.Format("{0}{1}", this.BaseUrl, requestUrl);
            var headers = new Dictionary<string, string>
            {
                { PaymentConstants.PaymentExtendedHttpHeaders.CorrelationId, traceActivityId.ActivityId.ToString() },
                { PaymentConstants.PaymentExtendedHttpHeaders.ApiVersion, apiVersion }
            };

            var response = await this.accountServiceHttpClient.SendAsync("GET", fullRequestUrl, traceActivityId, actionName, headers);
            string responseMessage = response.Content;

            if (response.IsSuccessStatusCode)
            {
                try
                {
                    return JsonConvert.DeserializeObject<T>(responseMessage);
                }
                catch
                {
                    throw TraceCore.TraceException(traceActivityId, new FailedOperationException("Failed to deserialize Account service response message."));
                }
            }
            else if (response.StatusCode == StatusCodes.Status400BadRequest)
            {
                throw TraceCore.TraceException(traceActivityId, new InvalidOperationException(string.Format("Receive a bad request response from Account service: {0}.", responseMessage ?? string.Empty)));
            }
            else
            {
                throw TraceCore.TraceException(traceActivityId, new FailedOperationException(string.Format("Received an error response from Account service, response status code: {0}, error: {1}", response.StatusCode, responseMessage != null ? responseMessage : string.Empty)));
            }
        }

        private async Task<T> SendPostRequest<T>(
            string url,
            string apiVersion,
            object request,
            string actionName,
            EventTraceActivity traceActivityId,
            string etag = null,
            Func<PXHttpResponse, EventTraceActivity, Task> errorHandler = null,
            bool regionIsoEnabled = false)
        {
            string fullRequestUrl = string.Format("{0}{1}", this.BaseUrl, url);
            var headers = new Dictionary<string, string>
            {
                { PaymentConstants.PaymentExtendedHttpHeaders.CorrelationId, traceActivityId.ActivityId.ToString() },
                { PaymentConstants.PaymentExtendedHttpHeaders.TrackingId, Guid.NewGuid().ToString() },
                { PaymentConstants.PaymentExtendedHttpHeaders.ApiVersion, apiVersion }
            };

            if (regionIsoEnabled)
            {
                headers.Add(AddressEnrichmentService.V7.Constants.ExtendedHttpHeaders.RegionIsoEnabled, Value.True);
            }

            // Etag and IfMatch are mandatory headers for account service V3.
            // For existing account service V2, etag is null by default and no extra headers are added.
            if (!string.IsNullOrEmpty(etag))
            {
                headers.Add(AccountService.V7.Constants.AccountV3ExtendedHttpHeaders.Etag, etag);
                headers.Add(AccountService.V7.Constants.AccountV3ExtendedHttpHeaders.IfMatch, etag);
            }

            string content = null;
            if (request != null)
            {
                content = JsonConvert.SerializeObject(request);
            }

            var response = await this.accountServiceHttpClient.SendAsync("POST", fullRequestUrl, traceActivityId, actionName, headers, content);
            string responseMessage = response.Content;

                if (response.IsSuccessStatusCode)
                {
                    try
                    {
                        return JsonConvert.DeserializeObject<T>(responseMessage);
                    }
                    catch
                    {
                        throw TraceCore.TraceException(traceActivityId, new FailedOperationException("Failed to deserialize success response from Accounts"));
                    }
                }
                else if (response.StatusCode == StatusCodes.Status400BadRequest)
                {
                    if (errorHandler != null)
                    {
                        await errorHandler(response, traceActivityId);
                    }

                    throw TraceCore.TraceException(traceActivityId, new InvalidOperationException(string.Format("Receive a bad request response from Account service: {0}.", responseMessage ?? string.Empty)));
                }
                else
                {
                    throw TraceCore.TraceException(traceActivityId, new FailedOperationException(string.Format("Received an error response from Account service, response status code: {0}, error: {1}", response.StatusCode, responseMessage != null ? responseMessage : string.Empty)));
                }
        }

        private async Task<T> SendRequest<T>(
            string url,
            string apiVersion,
            object request,
            string actionName,
            EventTraceActivity traceActivityId,
            string method,
            string etag = null,
            Func<PXHttpResponse, EventTraceActivity, Task> errorHandler = null)
        {
            string fullRequestUrl = string.Format("{0}{1}", this.BaseUrl, url);
            var headers = new Dictionary<string, string>
            {
                { PaymentConstants.PaymentExtendedHttpHeaders.CorrelationId, traceActivityId.ActivityId.ToString() },
                { PaymentConstants.PaymentExtendedHttpHeaders.TrackingId, Guid.NewGuid().ToString() },
                { PaymentConstants.PaymentExtendedHttpHeaders.ApiVersion, apiVersion }
            };

            // Etag and IfMatch are mandatory headers for account service V3.
            // For existing account service V2, etag is null by default and no extra headers are added.
            if (!string.IsNullOrEmpty(etag))
            {
                headers.Add(AccountService.V7.Constants.AccountV3ExtendedHttpHeaders.Etag, etag);
                headers.Add(AccountService.V7.Constants.AccountV3ExtendedHttpHeaders.IfMatch, etag);
            }

            string content = null;
            if (request != null)
            {
                string payload = JsonConvert.SerializeObject(
                    request,
                    Formatting.None,
                    new JsonSerializerSettings
                    {
                        NullValueHandling = NullValueHandling.Ignore
                    });
                content = payload;
            }

            var response = await this.accountServiceHttpClient.SendAsync(method, fullRequestUrl, traceActivityId, actionName, headers, content);
            string responseMessage = response.Content;

                if (response.IsSuccessStatusCode)
                {
                    try
                    {
                        return JsonConvert.DeserializeObject<T>(responseMessage);
                    }
                    catch
                    {
                        throw TraceCore.TraceException(traceActivityId, new FailedOperationException("Failed to deserialize success response from PIMS"));
                    }
                }
                else if (response.StatusCode == StatusCodes.Status400BadRequest)
                {
                    if (errorHandler != null)
                    {
                        await errorHandler(response, traceActivityId);
                    }

                    throw TraceCore.TraceException(traceActivityId, new InvalidOperationException(string.Format("Receive a bad request response from Account service: {0}.", responseMessage ?? string.Empty)));
                }
                else
                {
                    throw TraceCore.TraceException(traceActivityId, new FailedOperationException(string.Format("Received an error response from Account service, response status code: {0}, error: {1}", response.StatusCode, responseMessage != null ? responseMessage : string.Empty)));
                }
        }

        private async Task<T> GetProfileV3ByType<T>(string requestUrl, EventTraceActivity traceActivityId)
        {
            T profile = default(T);

            try
            {
                AccountProfilesV3<T> profiles = await this.SendGetRequest<AccountProfilesV3<T>>(
                    requestUrl,
                    GlobalConstants.AccountServiceApiVersion.V3,
                    "GetProfiles",
                    traceActivityId);
                if (profiles != null && profiles.UserProfiles != null)
                {
                    return profiles.UserProfiles.FirstOrDefault<T>();
                }
            }
            catch (FailedOperationException ex)
            {
                if (!ex.Message.Contains(StatusCodes.Status404NotFound.ToString()))
                {
                    throw;
                }
            }

            return profile;
        }
    }
}