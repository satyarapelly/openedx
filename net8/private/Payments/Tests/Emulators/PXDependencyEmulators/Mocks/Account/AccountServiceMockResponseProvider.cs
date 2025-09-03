// <copyright file="AccountServiceMockResponseProvider.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.Tests.Emulators.PXDependencyEmulators.Mocks
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Microsoft.Commerce.Payments.PXCommon;
    using Newtonsoft.Json;
    using Test.Common;
    using global::Tests.Common.Model;
    using global::Tests.Common.Model.Accounts;
    using Constants = Microsoft.Commerce.Payments.Tests.Emulators.PXDependencyEmulators.Constants;

    public class AccountServiceMockResponseProvider : IMockResponseProvider
    {
        static AccountServiceMockResponseProvider()
        {
            var profilesJson = File.ReadAllText(
                Path.Combine(
                    AppDomain.CurrentDomain.BaseDirectory,
                    "Mocks",
                    "Account",
                    "ProfilesV2ByAccountId.json"));

            ProfileV2ByAccountId = JsonConvert.DeserializeObject<Dictionary<string, List<ProfileV2>>>(profilesJson);

            var addressesJson = File.ReadAllText(
                Path.Combine(
                    AppDomain.CurrentDomain.BaseDirectory,
                    "Mocks",
                    "Account",
                    "AddressesV2ByAccountId.json"));

            AddressV2ByAccountId = JsonConvert.DeserializeObject<Dictionary<string, List<AddressV2>>>(addressesJson);

            var profilesV3Json = File.ReadAllText(
                Path.Combine(
                    AppDomain.CurrentDomain.BaseDirectory,
                    "Mocks",
                    "Account",
                    "ProfilesV3ByAccountId.json"));

            ProfileV3ByAccountId = JsonConvert.DeserializeObject<Dictionary<string, List<ProfileV3>>>(
                profilesV3Json,
                new JsonConverter[]
                {
                    new ProfileV3Deserializer()
                });

            var addressesV3Json = File.ReadAllText(
                Path.Combine(
                    AppDomain.CurrentDomain.BaseDirectory,
                    "Mocks",
                    "Account",
                    "AddressesV3ByAccountId.json"));
            AddressV3ByAccountId = JsonConvert.DeserializeObject<Dictionary<string, List<AddressV3>>>(addressesV3Json);

            var customerV3Json = File.ReadAllText(
                Path.Combine(
                    AppDomain.CurrentDomain.BaseDirectory,
                    "Mocks",
                    "Account",
                    "CustomersV3ByAccountId.json"));

            CustomerByAccountId = JsonConvert.DeserializeObject<Dictionary<string, List<CustomerInfo>>>(customerV3Json);
        }
        
        public static Dictionary<string, List<ProfileV2>> ProfileV2ByAccountId { get; private set; }

        public static Dictionary<string, List<AddressV2>> AddressV2ByAccountId { get; private set; }

        public static Dictionary<string, List<ProfileV3>> ProfileV3ByAccountId { get; private set; }

        public static Dictionary<string, List<AddressV3>> AddressV3ByAccountId { get; private set; }

        public static Dictionary<string, List<CustomerInfo>> CustomerByAccountId { get; private set; }

        public void ResetDefaults()
        {
        }

        public async Task<HttpResponseMessage> GetMatchedMockResponse(HttpRequestMessage request)
        {
            object result = null;

            // For SelfHosted env, MockServiceHandler sends request first to get the GetMatchedMockResponse instead of emulator controller
            // if the response from the GetMatchedMockResponse is null then Handler will send the request to emulator controller where it utilize the testScenarioHeaders or testContext
            // Returning null here to send the request to controller when the accounts service scenarioHeader is present
            bool isAccountServiceTestScenarioHeaderRequest = request.Headers.TryGetValues(Test.Common.Constants.HeaderValues.TestHeader, out var testHeaderValue) && testHeaderValue.FirstOrDefault().Contains(Constants.TestScenarios.PXAccount);

            // Ignore request from CIT for the logic to return null. Host name for CIT accounts service is mockAccountService
            bool isRequestFromCIT = string.Equals(request.RequestUri.Host, "mockAccountService", StringComparison.OrdinalIgnoreCase);

            if (isAccountServiceTestScenarioHeaderRequest && WebHostingUtility.IsApplicationSelfHosted() && !isRequestFromCIT)
            {
                return null;
            }

            if (request.Method == HttpMethod.Get)
            {
                string accountId = request.RequestUri.Segments[1].Trim(new char[] { '/' });
                string resource = request.RequestUri.Segments[2].Trim(new char[] { '/' });
                string resourceId = null;
                if (request.RequestUri.Segments.Length > 3)
                {
                    resourceId = request.RequestUri.Segments[3].Trim(new char[] { '/' });
                }

                bool isV3Request = request.Headers.GetValues("api-version").Contains("2015-03-31");

                if (string.Equals(resource, "profiles", StringComparison.OrdinalIgnoreCase))
                {
                    if (resourceId != null)
                    {
                        result = isV3Request ?
                            (object)ProfileV3ByAccountId[accountId].First(profile =>
                            {
                                return string.Equals(resourceId, profile.Id, StringComparison.OrdinalIgnoreCase);
                            }) :
                            (object)ProfileV2ByAccountId[accountId].First(profile =>
                            {
                                return string.Equals(resourceId, profile.Id, StringComparison.OrdinalIgnoreCase);
                            });
                    }
                    else
                    {
                        if (isV3Request)
                        {
                            var items = ProfileV3ByAccountId[accountId];
                            result = new ResponseV3<ProfileV3>() { Items = items, TotalCount = items.Count };
                        }
                        else
                        {
                            List<ProfileV2> profiles = null;
                            if (!ProfileV2ByAccountId.TryGetValue(accountId, out profiles))
                            {
                                profiles = new List<ProfileV2>();
                            }

                            result = new ResponseV2<ProfileV2>() { Items = profiles, ItemCount = profiles.Count.ToString() };
                        }
                    }
                }
                else if (string.Equals(resource, "addresses", StringComparison.OrdinalIgnoreCase))
                {
                    if (resourceId != null)
                    {
                        result = isV3Request ?
                            (object)AddressV3ByAccountId[accountId].First(address =>
                            {
                                return string.Equals(resourceId, address.Id, StringComparison.OrdinalIgnoreCase);
                            }) :
                            (object)AddressV2ByAccountId[accountId].First(address =>
                            {
                                return string.Equals(resourceId, address.Id, StringComparison.OrdinalIgnoreCase);
                            });
                    }
                    else
                    {
                        if (isV3Request)
                        {
                            var items = AddressV3ByAccountId[accountId];
                            result = new ResponseV3<AddressV3>() { Items = items, TotalCount = items.Count };
                        }
                        else
                        {
                            var items = AddressV2ByAccountId[accountId];
                            result = new ResponseV2<AddressV2>() { Items = items, ItemCount = items.Count.ToString() };
                        }
                    }
                }
                else if (string.Equals(accountId, "customers", StringComparison.OrdinalIgnoreCase))
                {
                            var items = CustomerByAccountId[accountId];
                            result = items.Where(x => x.Id == resource).First();
                }
            }
            else if (request.Method == HttpMethod.Post)
            {
                // Handle legacy address validation request
                // For now, the only Post scenarios are legacy address validation and PostAddress, we might need to add more if necessary
                string resource = request.RequestUri.Segments[1].Trim(new char[] { '/' });

                if (string.Equals(resource, "addresses", StringComparison.OrdinalIgnoreCase))
                {
                    string requestMessage = await request.Content.ReadAsStringAsync();
                    AddressV3 addressPayload = JsonConvert.DeserializeObject<AddressV3>(requestMessage);
                    if (!string.IsNullOrEmpty(addressPayload.AddressLine1))
                    {
                        result = "Valid";
                    }
                    else
                    {
                        result = "NotValid";
                    }
                }
                else
                {
                    // handle PostAddress request
                    string accountId = resource;
                    resource = request.RequestUri.Segments[2].Trim(new char[] { '/' });
                    if (string.Equals(resource, "addresses", StringComparison.OrdinalIgnoreCase)
                        && string.Equals(accountId, "AccountNoAddress", StringComparison.OrdinalIgnoreCase)
                        && string.Equals(request.Properties["InstrumentManagement.ActionName"].ToString(), "PostAddress", StringComparison.OrdinalIgnoreCase))
                    {
                        return await Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
                        {
                            Content = new StringContent(
                                "{\"id\":\"test-id-002\",\"country\":\"US\",\"region\":\"WA\",\"city\":\"Redmond\",\"address_line1\":\"One Microsoft Way\",\"address_line2\":\"\",\"address_line3\":\"\",\"postal_code\":\"98052\",\"links\":{\"self\":{\"href\":\"70f51631-0c04-41b0-9a59-5d8bf04bb9f5/addresses/ebc8a77b-4d5e-597e-11e6-6962671acadd\",\"method\":\"GET\"}},\"object_type\":\"Address\",\"contract_version\":\"2014-09-01\",\"resource_status\":\"Active\"}",
                                System.Text.Encoding.UTF8,
                                "application/json")
                        });
                    }
                    else if (string.Equals(resource, "profiles", StringComparison.OrdinalIgnoreCase)
                             && request.Properties != null
                             && string.Equals(accountId, "AccountNoAddress", StringComparison.OrdinalIgnoreCase)
                             && string.Equals(request.Properties["InstrumentManagement.ActionName"].ToString(), "UpdateProfile", StringComparison.OrdinalIgnoreCase)
                             && request.Properties.ContainsKey("Payments.Content")
                             && request.Properties["Payments.Content"] != null)
                    {
                        return await Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
                        {
                            Content = new StringContent(
                                "{\"first_name\":\"FirstName002\",\"last_name\":\"LastName002\",\"birth_date\":\"11/30/1995\",\"email_address\":\"test@email.com\",\"culture\":\"en-US\",\"country\":\"US\",\"account_id\":\"Account002\",\"id\":\"Profile002002\",\"snapshot_id\":\"snapshot-account002002/3\",\"type\":\"consumer\",\"default_address_id\":\"test-id-002\",\"links\":{\"self\":{\"href\":\"Account002/profiles/snapshot-account002002/3\",\"method\":\"GET\"},\"snapshot\":{\"href\":\"Account002/profiles/snapshot-account002002/3\",\"method\":\"GET\"},\"update\":{\"href\":\"Account002/profiles/snapshot-account002002\",\"method\":\"POST\"},\"default_address\":{\"href\":\"Account002/addresses/test-id-002\",\"method\":\"GET\"}},\"object_type\":\"ConsumerProfile\",\"contract_version\":\"2014-09-01\",\"resource_status\":\"Active\"}",
                                System.Text.Encoding.UTF8,
                                "application/json")
                        });
                    }
                    else if (string.Equals(resource, "get-or-create-legacy-billable-account", StringComparison.OrdinalIgnoreCase))
                    {
                        return await Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
                        {
                            Content = new StringContent(
                                "{\"id\":\"QEnSBQAAAAAAAAAA\",\"bdk_id\":\"QEnSBQAAAAAAAAAA\",\"cid\":\"a1b399b5-8ac0-48fe-9b24-6741cdefa9fa\",\"first_name\":\"Test\",\"last_name\":\"Test\",\"country_code\":\"US\",\"account_level\":\"primary\",\"customer_type\":\"personal\",\"source\":\"scs\",\"last_updated_date\":\"10/4/2021 2:10:55 am\",\"profile_type\":\"consumer\",\"identity\":{\"type\":\"PUID\",\"value\":\"985160790146282\"},\"is_azure\":false,\"object_type\":\"LegacyBillableAccount\",\"resource_status\":\"Active\"}",
                                System.Text.Encoding.UTF8,
                                "application/json")
                        });
                    }
                    else
                    {
                        result = "NotValid";
                    }
                }
            }
            
            return await Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(
                    JsonConvert.SerializeObject(result),
                    System.Text.Encoding.UTF8,
                    "application/json")
            });
        }
    }
}