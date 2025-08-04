// <copyright file="PaymentTransactionsController.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.V7.PaymentTransaction
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Text.Json;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using Common;
    using Common.Tracing;
    using Common.Web;
    using Microsoft.Commerce.Payments.Pidl.Localization;
    using Microsoft.Commerce.Payments.PidlFactory.V7;
    using Microsoft.Commerce.Payments.PXService.V7;
    using Microsoft.Extensions.DependencyInjection;
    using Model;
    using Newtonsoft.Json.Linq;
    using PXCommon;
    using static Microsoft.Commerce.Payments.PXService.V7.Constants;
    using Catalog = PXService.Model.CatalogService;
    using ClientActionContract = GlobalConstants.ClientActionContract;
    using D365 = PXService.Model.D365Service;
    using PIMSModel = Microsoft.Commerce.Payments.PimsModel.V4;
    using Purchase = PXService.Model.PurchaseService;
    using System.Diagnostics.Tracing;

    /// <summary>
    /// Gets orders and subscriptions from M$ and D365, legacy subscriptions from CTP, product details from 
    /// Catalog, PIs from PIMS, and combines them in a way that's needed by clients like North Star.
    /// North Star Url:
    /// https://account.microsoft.com/billing/payments/
    /// M$ API Reference:
    /// https://swagger/service/2170173e-1241-4eaf-a845-caf8989be27a/
    /// Catalog API Reference:
    /// <![CDATA[https://microsoft.sharepoint.com/teams/CatalogPurchaseUseTeam/_layouts/15/Doc.aspx?sourcedoc={6e345ceb-676d-41f5-b3af-312cb33a35a0}&action=edit&wd=target%28Catalog%2F03.%20DCatFD%20APIs%2FV8%20API.one%7C93c343c6-3420-454f-8cde-f857398ba70e%2FDomain%20Data%20API%7C3def0b0c-0c5e-4045-ad62-601dd7436b3c%2F%29]]>
    /// </summary>
    public class PaymentTransactionsController : ProxyController
    {
        // M$'s GET /v7.0/users/{userId}/orders supports pagination.  This is the nuber of orders we want to get
        // per page. If this number is too small, we will need to make multiple network calls to M$.  If its too
        // big, each call could take a long time.  IIRC, we arrived at 100 based on trial-and-error to get what
        // seemed like a good tradeoff. 
        private const int PageSizeTransactions = 100;

        // We want to get a max of 200 (TransactionOrderCountCap) orders in the last 2 months.  This number used
        // to be 1000 but was reduced to 200 to reduce latency (some users for some reason have a very large number
        // of orders).  
        // TODO: In the case where a user has > 200 orders in the last two months, not sure if the M$ API returns 
        // the most recent 200 orders (if its not gauranteed to be the most-recent, it could cause user-confusion).
        // Check with M$ team about this API's behavior
        private const int TransactionOrderCountCap = 200;

        // To decide if a user can delete a PI, we rely on M$'s CheckPI API
        // (/v7.0/users/{userId}/paymentinstruments/{paymentinstrumentid}/check).  However, North Star needs this info
        // at startup for every PI.  So, if a user has a large number of PIs on file, this can generate a lot of load
        // (and potentially allow a DOS attack?).  Hence capping to 5.  Worst case scenario, the user tries to delete
        // the 6th PI and they get an error during deletion.
        private const int CheckPiCap = 5;

        private static readonly string[] defaultStatusToQuery = new string[] { "active" };
        private static readonly List<string> filteredPiFamilies = new List<string> { "online_bank_transfer", "offline_bank_transfer" };
        private static readonly List<string> filteredPiTypes = new List<string> { "paytrail", "eps", "giropay", "dotpay", "mpesa", "ideal", "sandbox_check", "ea_check", "poli", "trustly", "sofort", "check" };

        /// <summary>
        /// List Payment Transactions for the current user
        /// </summary>
        /// <param name="accountId">Pi Account Id</param>
        /// <param name="continuationToken">
        ///     ContinuationToken for getting additional transactions.  This parameter was added to support pagination
        ///     on the North Star UI - A button to "Show More".  However, this is not being used currently (Sep 2021).
        ///     Instead, on North Star, we show a scrollable list of orders with a fixed max limit 
        ///     determined by the const "TransactionOrderCountCap".
        /// </param>
        /// <param name="status">Pi status</param>
        /// <param name="deviceId">Pi device Id</param>
        /// <param name="language">Pi language</param>
        /// <param name="partner">Pi partner</param>
        /// <param name="country">Pi country</param>
        /// <returns>Returns PaymentTransactions containing orders and subscriptions.  Orders and subscriptions
        /// reference the PIID that is associated with them.  In addition, Orders also have a checkPiResult boolean
        /// indicating if the associated PI should be blocked from deletion (e.g. a hardware order which has not yet
        /// shipped and hence funds have been authorized but not captured yet.)</returns>
        [HttpGet]
        public async Task<PaymentTransactions> ListTransactions(
            string accountId,
            string continuationToken = null,
            string[] status = null,
            ulong deviceId = 0,
            string language = "en",
            string partner = V7.Constants.ServiceDefaults.DefaultPartnerName,
            string country = null)
        {
            status = (status == null || status.Length == 0) ? defaultStatusToQuery : status;
            PaymentTransactions transcations = await this.ListTransactionsFromCheckPI(accountId, continuationToken, status, deviceId, language, partner, country);
            return transcations;
        }

        /// <summary>
        /// List Payment Transactions for the current user
        /// </summary>
        /// <param name="accountId">Pi Account Id</param>
        /// <param name="country">Pi country</param>
        /// <param name="language">Pi language</param>
        /// <param name="partner">Pi partner</param>
        /// <param name="requestData"> Piid and Tokenized CVV</param>
        /// <returns>Returns PaymentTransactions containing orders and subscriptions.  Orders and subscriptions
        /// reference the PIID that is associated with them.  In addition, Orders also have a checkPiResult boolean
        /// indicating if the associated PI should be blocked from deletion (e.g. a hardware order which has not yet
        /// shipped and hence funds have been authorized but not captured yet.)</returns>
        [HttpPost]
        public async Task<HttpResponseMessage> ListTransactions(string accountId, string country, string language, string partner, [FromBody] PIDLData requestData)
        {
            // NOTE Add traces and logs
            EventTraceActivity traceActivityId = this.Request.GetRequestCorrelationId();
            var piid = requestData.TryGetPropertyValue("id");
            this.Request.AddTracingProperties(accountId, piid, null, null);
            PaymentTransactions transactions = null;
            PIMSModel.PaymentInstrument currentPaymentInstrument = null;

            try
            {
                // Form payload to validate cvv
                object payload = new
                {
                    paymentInstrumentOperation = Operations.SearchTransactions,
                    paymentInstrumentCountry = country,
                    cvvToken = requestData.TryGetPropertyValue("cvvToken")
                };

                object response = await this.Settings.PIMSAccessor.ValidateCvv(accountId, piid, payload, traceActivityId);

                if (response == null)
                {
                    // Call PIMS to get current MSA payment instrument details
                    currentPaymentInstrument = await this.Settings.PIMSAccessor.GetPaymentInstrument(accountId, piid, traceActivityId, partner, country, language, this.ExposedFlightFeatures);

                    // Form PIDL data to call PIMS SearchByAccountNumber
                    var piPayload = new
                    {
                        filters = new { paymentInstrumentId = piid }
                    };

                    // Call PIMS to get account number by piid
                    // Call PIMS to get all account number and its piid which is using current msa pi
                    List<SearchTransactionAccountinfoByPI> accountinfoByPI = await this.Settings.PIMSAccessor.SearchByAccountNumber(piPayload, traceActivityId);

                    // Below if else block added to the scenario when PIMS SearchByAccountNumber API not returning current PI if its added to MSA less than 72 hrs. 
                    // if block executes on the scenario, PIMS returns null or empty, if selected PI was added to the MSA with in 72 hours not used in other MSA's.
                    // Else block executes on the scenario, when PIMS returns only other MSA's and PI's not the selected PI since it as added with in 72 hours
                    if (accountinfoByPI == null || !accountinfoByPI.Any())
                    {
                        accountinfoByPI = new List<SearchTransactionAccountinfoByPI> { new SearchTransactionAccountinfoByPI { PaymentInstrumentId = piid, PaymentInstrumentAccountId = accountId } };
                    }
                    else if (!accountinfoByPI.Where(x => x.PaymentInstrumentAccountId == accountId && x.PaymentInstrumentId == piid).Any())
                    {
                        accountinfoByPI.Add(new SearchTransactionAccountinfoByPI { PaymentInstrumentId = piid, PaymentInstrumentAccountId = accountId });
                    }

                    if (this.ExposedFlightFeatures.Contains(Flighting.Features.PXEnableSearchTransactionParallelRequest, StringComparer.OrdinalIgnoreCase))
                    {
                        transactions = await this.ListTransactions(accountId, partner, language, country, traceActivityId, accountinfoByPI, transactions);
                        transactions.PaymentInstrument = currentPaymentInstrument;
                    }
                    else
                    {
                        transactions = await this.ListTransactionsNotOptimized(accountId, partner, language, country, traceActivityId, piid, accountinfoByPI, transactions, currentPaymentInstrument);
                    }

                    // sort the return order items
                    transactions?.Orders?.Sort((x, y) => y.OrderedDate.CompareTo(x.OrderedDate));

                    return this.Request.CreateResponse(transactions);
                }
                else
                {
                    // validateCvv returns 204 on success, if content was received, there was some error
                    var err = new ErrorMessage()
                    {
                        ErrorCode = "ValidateCVVReturnedContent",
                        Message = "Validate CVV returned 200 instead of 204",
                        Retryable = false,
                    };

                    return this.Request.CreateResponse(HttpStatusCode.BadRequest, err, "application/json");
                }
            }
            catch (ServiceErrorResponseException ex)
            {
                if (string.Equals(ex.Error.ErrorCode, ValidateCvvErrorCodes.InvalidCvv, StringComparison.OrdinalIgnoreCase)
                    || string.Equals(ex.Error.ErrorCode, CupErrorCodes.ValidationFailed, StringComparison.OrdinalIgnoreCase))
                {
                    ex.Error.Message = ClientActionContract.NoMessage;
                    ex.Error.ErrorCode = CupErrorCodes.ValidationFailed;
                    ex.Error.AddDetail(new ServiceErrorDetail()
                    {
                        ErrorCode = CupErrorCodes.ValidationFailed,
                        Message = LocalizationRepository.Instance.GetLocalizedString(CreditCardErrorMessages.PIMSValidationFailed, language),
                        Target = ValidateCvvErrorTargets.Cvv,
                    });
                }
                else
                {
                    ex.Error.Message = LocalizationRepository.Instance.GetLocalizedString(CreditCardErrorMessages.Generic, language);
                }

                return this.Request.CreateResponse(ex.Response.StatusCode, ex.Error, "application/json");
            }
        }

        private static string ConvertToNextLink(string continuationToken)
        {
            var nextLinkBytes = System.Convert.FromBase64String(continuationToken); // lgtm[cs/base64-decoding-without-validation] Suppressing Semmle warning
            return System.Text.Encoding.UTF8.GetString(nextLinkBytes);
        }

        private static string ConvertToContinuationToken(string nextLink)
        {
            var nextLinkBytes = System.Text.Encoding.UTF8.GetBytes(nextLink);
            return System.Convert.ToBase64String(nextLinkBytes);
        }

        private static Dictionary<string, bool> GetOrderIdToPaymentInUse(List<Purchase.PaymentInstrumentCheckResponse> piCheckResponses)
        {
            var orderIdToPaymentInUse = new Dictionary<string, bool>();
            foreach (var piCheckResponse in piCheckResponses)
            {
                foreach (var orderId in piCheckResponse.OrderIds)
                {
                    if (!orderIdToPaymentInUse.ContainsKey(orderId))
                    {
                        orderIdToPaymentInUse.Add(orderId, piCheckResponse.PaymentInstrumentInUse);
                    }
                }
            }

            return orderIdToPaymentInUse;
        }

        private async Task<PaymentTransactions> ListTransactionsFromCheckPI(
            string accountId,
            string continuationToken,
            string[] status,
            ulong deviceId,
            string language,
            string partner,
            string country,
            string puid = null,
            IEnumerable<PIMSModel.PaymentInstrument> targetedPis = null,
            string operation = null)
        {
            EventTraceActivity traceActivityId = this.Request.GetRequestCorrelationId();
            string email = string.Empty;

            if (operation != Operations.SearchTransactions)
            {
                puid = await this.TryGetClientContext(GlobalConstants.ClientContextKeys.MsaProfile.Puid);

                if (string.IsNullOrWhiteSpace(puid))
                {
                    throw new ValidationException(ErrorCode.InvalidAccountId, "PUID is invalid");
                }

                email = await this.TryGetClientContext(GlobalConstants.ClientContextKeys.MsaProfile.EmailAddress);

                if (string.IsNullOrWhiteSpace(email))
                {
                    throw new ValidationException(ErrorCode.EmailNotFound, "Email address is invalid");
                }
            }

            string msaPuid = $"msa:{puid}";

            PaymentTransactions trans = new PaymentTransactions();
            trans.PageSize = PageSizeTransactions;

            if (string.IsNullOrWhiteSpace(continuationToken))
            {
                // The basic logic is:
                // 1. List all eligible PIs under the account from PIMS since the PIs are used by both of Purchase and D365.
                // 2. List all transactions from Purchase and all transactions from D365.
                if (operation != Operations.SearchTransactions)
                {
                    targetedPis = await this.ListEligiblePIs(accountId: accountId, status: status, deviceId: deviceId, language: language, partner: partner, country: country, traceActivityId: traceActivityId);
                }

                var listTransactionsTasks = new List<Task>();
                listTransactionsTasks.Add(this.ListTransactionsFromPurchase(trans, targetedPis, puid, traceActivityId));

                // No need to call Catalog service to get product details since D365 orders have done this.
                // All orders from D365 only list in the first page. 
                if (this.ExposedFlightFeatures.Contains(Flighting.Features.PXEnableListD365PendingOrders, StringComparer.OrdinalIgnoreCase))
                {
                    listTransactionsTasks.Add(this.ListTransactionsFromD365(trans, targetedPis, puid, traceActivityId));
                }

                await Task.WhenAll(listTransactionsTasks);
            }
            else
            {
                var nextLink = ConvertToNextLink(continuationToken);
                await this.FetchAllOrders(puid: msaPuid, nextLink: nextLink, trans: trans, traceActivityId: traceActivityId);

                // for 2nd page on, we just set CheckPiResult to false for all orders to avoid CheckPiResult call
                trans.Orders.ForEach(order => order.CheckPiResult = false);
                await this.PopulateProductIdsForOrders(trans, traceActivityId);
            }

            if (operation != Operations.SearchTransactions)
            {
                trans.Orders.ForEach(order => order.Email = email);
            }

            // sort the return order items
            trans.Orders.Sort((x, y) => y.OrderedDate.CompareTo(x.OrderedDate));

            return trans;
        }

        private async Task ListTransactionsFromPurchase(PaymentTransactions trans, IEnumerable<PIMSModel.PaymentInstrument> targetedPis, string puid, EventTraceActivity traceActivityId)
        {
            string msaPuid = $"msa:{puid}";

            // Parallelly start three tasks:
            // 1. Call ListOrder to fetch all orders up to the TransactionOrderCountCap
            // 2. Call ListSubscription to fetch all the subscriptions
            // 3. For each PI returning from PIMS call CheckPaymentInstrument to get each PI's subs and orders
            var orderIdsFromListOrdersTask = this.ListOrders(trans: trans, msaPuid: msaPuid, traceActivityId: traceActivityId);
            var subscriptionIdsFromListSubscriptionsTask = this.ListSubscriptions(trans: trans, userId: msaPuid, maxPageSize: PageSizeTransactions, traceActivityId: traceActivityId);
            var piCheckResponsesTask =
                this.CheckPIResultForPurchase(
                    msaPuid: msaPuid,
                    targetedPis,
                    traceActivityId: traceActivityId);
            await Task.WhenAll(orderIdsFromListOrdersTask, subscriptionIdsFromListSubscriptionsTask, piCheckResponsesTask);
            var piCheckResponses = piCheckResponsesTask.Result;
            var orderIdToPaymentInUse = GetOrderIdToPaymentInUse(piCheckResponses);
            var blockingPIOrderIds = piCheckResponses.Where(p => p.PaymentInstrumentInUse).Where(p => p.OrderIds != null).SelectMany(p => p.OrderIds).Distinct().ToList();
            var blockingPISubIds = piCheckResponses.Where(p => p.PaymentInstrumentInUse).Where(p => p.RecurrenceIds != null).SelectMany(p => p.RecurrenceIds).Distinct().ToHashSet();

            var orderIdsFromListOrders = orderIdsFromListOrdersTask.Result;
            var blockingPIOrders = blockingPIOrderIds.Except(orderIdsFromListOrders).ToList();
            await this.PopulateBlockingOrderAndSubIds(trans: trans, puid: puid, pendingOrderDiff: blockingPIOrders, traceActivityId: traceActivityId, blockingPISubscriptionIds: blockingPISubIds);

            trans.PopulateBlockingPiResultForOrders(orderIdToPaymentInUse);
            trans.PopulateBlockingPiResultForSubs(blockingPISubIds);

            this.PopulateCtpSubscriptions(trans, puid, traceActivityId);
            await this.PopulateProductIdsForOrders(trans, traceActivityId);
        }

        private async Task ListTransactionsFromD365(PaymentTransactions trans, IEnumerable<PIMSModel.PaymentInstrument> targetedPis, string puid, EventTraceActivity traceActivityId)
        {
            try
            {
                string msaPuid = $"msa:{puid}";
                var piCheckResponsesTask =
                    this.CheckPIResultForD365(
                        msaPuid: msaPuid,
                        targetedPis,
                        traceActivityId: traceActivityId);
                await Task.WhenAll(piCheckResponsesTask);

                var piCheckResponses = piCheckResponsesTask.Result;
                var pendingOrderIds = piCheckResponses.Where(p => p.PendingOrderIds != null).SelectMany(p => p.PendingOrderIds).Distinct().ToList();

                await this.PopulateD365PendingOrders(trans: trans, puid: puid, pendingOrderIds: pendingOrderIds, traceActivityId: traceActivityId);
            }
            catch (Exception ex)
            {
                // intentional as we don't want to break the flow of purchase.
                SllWebLogger.TracePXServiceException(
                    string.Format(
                        "List transactions from D365 failed - Message {0}, Trace {1}",
                        ex.Message,
                        ex.StackTrace.Substring(0, Math.Min(2000, ex.StackTrace.Length))),
                    traceActivityId);
            }
        }

        private async Task ListSubscriptions(PaymentTransactions trans, string userId, int maxPageSize, EventTraceActivity traceActivityId)
        {
            Purchase.Subscriptions subscriptions = await this.Settings.PurchaseServiceAccessor.ListSubscriptions(
                userId: userId,
                maxPageSize: maxPageSize,
                traceActivityId: traceActivityId);

            trans.PopulateSubscriptions(subscriptions.Items);
        }

        // TODO: This should be changed to TryPopulateCtpSubscriptions so that if CTP is down, we can at least return
        // orders and subscriptions from M$
        private void PopulateCtpSubscriptions(PaymentTransactions trans, string puid, EventTraceActivity traceActivityId)
        {
            var payinAccountIds = LegacyAccountHelper.GetLegacyBillablePayinAccountIds(
                    this.Settings,
                    traceActivityId,
                    puid,
                    GlobalConstants.Defaults.Language);

            var ctpSubscriptions = CTPCommerceHelper.GetSubscriptions(
                    this.Settings,
                    payinAccountIds,
                    puid,
                    traceActivityId,
                    GlobalConstants.Defaults.Language);

            trans.PopulateCtpSubscriptions(ctpSubscriptions);
        }

        private async Task PopulateBlockingOrderAndSubIds(
            PaymentTransactions trans,
            string puid,
            List<string> pendingOrderDiff,
            EventTraceActivity traceActivityId,
            HashSet<string> blockingPISubscriptionIds)
        {
            var getOrderTasks = new List<Task<Purchase.Order>>();
            foreach (var pendingOrder in pendingOrderDiff)
            {
                getOrderTasks.Add(this.Settings.PurchaseServiceAccessor.GetOrder(
                                puid: puid,
                                orderId: pendingOrder,
                                traceActivityId: traceActivityId));
            }

            await Task.WhenAll(getOrderTasks);

            foreach (var getOrderTask in getOrderTasks)
            {
                trans.PopulatePurchaseOrder(getOrderTask.Result);
                blockingPISubscriptionIds.Add(getOrderTask.Result.OrderLineItems[0].RecurrenceId);
            }
        }

        private async Task PopulateProductIdsForOrders(PaymentTransactions trans, EventTraceActivity traceActivityId)
        {
            try
            {
                const int BatchSize = 25;
                List<string> productIds = trans.GetProductIds();
                if (productIds.Count > 0)
                {
                    List<Task<Catalog.Catalog>> catalogTasks = new List<Task<Catalog.Catalog>>();
                    int batches = (int)Math.Ceiling((decimal)productIds.Count / BatchSize);
                    for (int i = 0; i < batches; i++)
                    {
                        int rangeStart = i * BatchSize;
                        var rangeEnd = productIds.Count - rangeStart < BatchSize ? productIds.Count - rangeStart : BatchSize;
                        var productIdSubList = productIds.GetRange(i * BatchSize, rangeEnd);

                        // TODO: This looks like a bug.  Confirm with PM that we want to show localized product titles.
                        // If yes, check with the Catalog team if we can pass user's country and language parameters
                        // instead of hardcoding them.  ListTransactions action above already accepts these parameters
                        // from clients.
                        var catalogTask = this.Settings.CatalogServiceAccessor.GetProducts(productIdSubList, "us", "en", "BrowseData", null, traceActivityId);
                        catalogTasks.Add(catalogTask);
                    }

                    await Task.WhenAll(catalogTasks);
                    foreach (var catalogTask in catalogTasks)
                    {
                        trans.PopulateProductNames(catalogTask.Result);
                    }
                }
            }
            catch (Exception ex)
            {
                // intentional as we don't want to break
                SllWebLogger.TracePXServiceException(
                    string.Format(
                        "Product Catalog lookup failed - Message {0}, Trace {1}",
                        ex.Message,
                        ex.StackTrace.Substring(0, Math.Min(2000, ex.StackTrace.Length))),
                    traceActivityId);
            }
        }

        private async Task PopulateD365PendingOrders(
            PaymentTransactions trans,
            string puid,
            List<string> pendingOrderIds,
            EventTraceActivity traceActivityId)
        {
            var getOrderTasks = new List<Task<D365.PagedResponse<D365.Order>>>();
            foreach (var pendingOrderId in pendingOrderIds)
            {
                getOrderTasks.Add(this.Settings.D365ServiceAccessor.GetOrder(
                                puid: puid,
                                orderId: pendingOrderId,
                                traceActivityId: traceActivityId));
            }

            await Task.WhenAll(getOrderTasks);

            foreach (var getOrderTask in getOrderTasks)
            {
                // Make sure there is an order 
                if (getOrderTask.Result.Items != null && getOrderTask.Result.Items.Count > 0)
                {
                    trans.PopulateD365Order(getOrderTask.Result.Items[0]);
                }

                if (getOrderTask.Result?.ApiErrors != null && getOrderTask.Result.ApiErrors.Count > 0)
                {
                    SllLogger.TraceMessage($"Partial error for the user {puid} when getting orders from Dynamics 365. Error Code: {getOrderTask.Result.ApiErrors[0].Code}. Error Message: {getOrderTask.Result.ApiErrors[0].Message}", EventLevel.Error);
                }
            }
        }

        private async Task<IEnumerable<PIMSModel.PaymentInstrument>> ListEligiblePIs(
            string accountId,
            string[] status,
            ulong deviceId,
            string language,
            string partner,
            string country,
            EventTraceActivity traceActivityId)
        {
            List<KeyValuePair<string, string>> queryParams = new List<KeyValuePair<string, string>>();
            if (!string.IsNullOrWhiteSpace(country))
            {
                queryParams.Add(new KeyValuePair<string, string>(V7.Constants.QueryParameterName.Country, country));
            }

            PIMSModel.PaymentInstrument[] paymentInstruments = await this.Settings.PIMSAccessor.ListPaymentInstrument(
                accountId: accountId,
                deviceId: deviceId,
                status: status,
                traceActivityId: traceActivityId,
                queryParams: queryParams,
                partner: partner,
                country: country,
                language: language,
                exposedFlightFeatures: this.ExposedFlightFeatures);

            IEnumerable<PIMSModel.PaymentInstrument> targetedPis = paymentInstruments?.Where(pi => !filteredPiFamilies.Contains(pi.PaymentMethod.PaymentMethodFamily)
                    && !filteredPiTypes.Contains(pi.PaymentMethod.PaymentMethodType));

            return targetedPis;
        }

        private async Task<List<Purchase.PaymentInstrumentCheckResponse>> CheckPIResultForPurchase(
            string msaPuid,
            IEnumerable<PIMSModel.PaymentInstrument> targetedPis,
            EventTraceActivity traceActivityId)
        {
            var checkPiTasks = new List<Task<Purchase.PaymentInstrumentCheckResponse>>();
            var piCheckResponses = new List<Purchase.PaymentInstrumentCheckResponse>();

            if (targetedPis != null && targetedPis.Count() != 0)
            {
                SllLogger.TraceMessage($"CheckPiTasks for Purchase Count: {targetedPis.Count()}.", EventLevel.Informational);

                foreach (var pi in targetedPis)
                {
                    if (checkPiTasks.Count > CheckPiCap)
                    {
                        break;
                    }

                    var checkPiTask = this.Settings.PurchaseServiceAccessor.CheckPaymentInstrument(
                        userId: msaPuid,
                        piId: pi.PaymentInstrumentId,
                        traceActivityId: traceActivityId);

                    checkPiTasks.Add(checkPiTask);
                }

                await Task.WhenAll(checkPiTasks);

                foreach (var checkPiTask in checkPiTasks)
                {
                    piCheckResponses.Add(checkPiTask.Result);
                }
            }

            return piCheckResponses;
        }

        private async Task<List<D365.PaymentInstrumentCheckResponse>> CheckPIResultForD365(
            string msaPuid,
            IEnumerable<PIMSModel.PaymentInstrument> targetedPis,
            EventTraceActivity traceActivityId)
        {
            var checkPiTasks = new List<Task<D365.PaymentInstrumentCheckResponse>>();
            var piCheckResponses = new List<D365.PaymentInstrumentCheckResponse>();

            if (targetedPis != null && targetedPis.Count() != 0)
            {
                SllLogger.TraceMessage($"CheckPiTasks for D365 Count: {targetedPis.Count()}.", EventLevel.Informational);

                foreach (var pi in targetedPis)
                {
                    if (checkPiTasks.Count > CheckPiCap)
                    {
                        break;
                    }

                    var checkPiTask = this.Settings.D365ServiceAccessor.CheckPaymentInstrument(
                        userId: msaPuid,
                        piId: pi.PaymentInstrumentId,
                        traceActivityId: traceActivityId);

                    checkPiTasks.Add(checkPiTask);
                }

                await Task.WhenAll(checkPiTasks);

                foreach (var checkPiTask in checkPiTasks)
                {
                    piCheckResponses.Add(checkPiTask.Result);
                }
            }

            return piCheckResponses;
        }

        private async Task<List<string>> ListOrders(PaymentTransactions trans, string msaPuid, EventTraceActivity traceActivityId)
        {
            var startTime = DateTime.UtcNow.AddMonths(-2);
            var endTime = DateTime.UtcNow.AddDays(1);

            // get all orders up to the TransactionOrderCountCap
            Purchase.Orders orders = await this.Settings.PurchaseServiceAccessor.ListOrders(
                userId: msaPuid,
                maxPageSize: PageSizeTransactions,
                startTime: startTime,
                endTime: endTime,
                validOrderStates: PaymentTransactions.GetValidOrderStates().ToList(),
                traceActivityId: traceActivityId);

            trans.PopulateOrders(orders.Items);

            // continue to get all orders before we can cross check with orders in CheckPiResult
            if (!string.IsNullOrWhiteSpace(orders.NextLink) && trans.Orders.Count < TransactionOrderCountCap)
            {
                await this.FetchAllOrders(puid: msaPuid, nextLink: orders.NextLink, trans: trans, traceActivityId: traceActivityId);
            }

            return trans.Orders.Select(order => order.OrderId).ToList();
        }

        private async Task FetchAllOrders(string puid, string nextLink, PaymentTransactions trans, EventTraceActivity traceActivityId)
        {
            Purchase.Orders orders = await this.Settings.PurchaseServiceAccessor.ListOrders(
                userId: puid,
                nextLink: nextLink,
                traceActivityId: traceActivityId,
                this.ExposedFlightFeatures);

            trans.PopulateOrders(orders.Items);

            //// bail if the account has more than this amount of transactions
            trans.ContinuationToken = null;
            if (!string.IsNullOrWhiteSpace(orders.NextLink) && trans.Orders.Count < TransactionOrderCountCap)
            {
                trans.ContinuationToken = ConvertToContinuationToken(orders.NextLink);
                await this.FetchAllOrders(puid: puid, nextLink: orders.NextLink, trans: trans, traceActivityId: traceActivityId);
            }
        }

        // function ListTransactionsNotOptimized doesn't have parallel calls i.e for profile and email we are calling one after other
        private async Task<PaymentTransactions> ListTransactionsNotOptimized(string accountId, string partner, string language, string country, EventTraceActivity traceActivityId, string piid, List<SearchTransactionAccountinfoByPI> accountinfoByPI, PaymentTransactions transactions, PIMSModel.PaymentInstrument currentPaymentInstrument)
        {
            string loggedInUserEmail = string.Empty;

            foreach (var accountInfo in accountinfoByPI)
            {
                var paymentInstrument = await this.Settings.PIMSAccessor.GetPaymentInstrument(accountInfo.PaymentInstrumentAccountId, accountInfo.PaymentInstrumentId, traceActivityId, partner, country, language, this.ExposedFlightFeatures);
                IEnumerable<PIMSModel.PaymentInstrument> targetedPis = new List<PIMSModel.PaymentInstrument>() { paymentInstrument };
                var email = string.Empty;

                if (piid == accountInfo.PaymentInstrumentId)
                {
                    currentPaymentInstrument = paymentInstrument;
                }

                // Call Jarvis or Account service to get puid and profile type
                var customer = await this.Settings.AccountServiceAccessor.GetCustomers(accountInfo.PaymentInstrumentAccountId, traceActivityId);
                var puid = customer?.Identity?.Data?.PUID;

                // Call Jarvis or Account service to get profile using profile type from get customer call
                try
                {
                    var profile = await this.Settings.AccountServiceAccessor.GetProfile(accountInfo.PaymentInstrumentAccountId, GlobalConstants.ProfileTypes.Consumer, traceActivityId);

                    if (accountId != accountInfo.PaymentInstrumentAccountId && profile != null && profile.EmailAddress != null)
                    {
                        email = JsonDataMasker.DelegateMaskEmailWithoutLength(JToken.Parse(Newtonsoft.Json.JsonConvert.SerializeObject(profile.EmailAddress))).ToString();
                    }
                    else
                    {
                        email = profile?.EmailAddress;
                        loggedInUserEmail = email;
                    }
                }
                catch (Exception ex)
                {
                    email = string.Empty;
                    SllWebLogger.TracePXServiceException($"{Operations.SearchTransactions} - jarvis: GetProfile failed/return exception while getting Userprofile information. " + ex, traceActivityId);
                }

                if (!string.IsNullOrWhiteSpace(puid))
                {
                    PaymentTransactions accountLevelTransaction = await this.ListTransactionsFromCheckPI(accountInfo.PaymentInstrumentAccountId, null, null, 0, language, partner, country, puid, targetedPis, Operations.SearchTransactions);

                    if (accountLevelTransaction.Orders != null)
                    {
                        accountLevelTransaction.Orders = accountLevelTransaction.Orders
                            .Where(x => x.Piid == accountInfo.PaymentInstrumentId)
                            .ToList();

                        foreach (var x in accountLevelTransaction.Orders.Where(x => x.UserId == puid))
                        {
                            x.Email = email;
                        }

                        if (transactions == null)
                        {
                            transactions = accountLevelTransaction;
                        }
                        else
                        {
                            transactions.Orders.AddRange(accountLevelTransaction.Orders);
                        }
                    }
                }
            }

            if (transactions == null)
            {
                transactions = new PaymentTransactions();
                transactions.PageSize = PageSizeTransactions;
            }

            transactions.Email = loggedInUserEmail;
            transactions.PaymentInstrument = currentPaymentInstrument;

            return transactions;
        }

        // created new function optimisation of code lines
        private async Task<PaymentTransactions> ListTransactions(string accountId, string partner, string language, string country, EventTraceActivity traceActivityId, List<SearchTransactionAccountinfoByPI> accountinfoByPI, PaymentTransactions transactions)
        {
            string loggedInUserEmail = string.Empty;

            var searchTransactionRequests = new List<SearchTransaction>();
            {
                var uniqueAccountIds = accountinfoByPI.Select(x => x.PaymentInstrumentAccountId).Distinct().ToList();

                foreach (var uniqueAccountId in uniqueAccountIds)
                {
                    searchTransactionRequests.Add(new SearchTransaction
                    {
                        PaymentInstrumentAccountId = uniqueAccountId,
                        PaymentInstruments = accountinfoByPI.Where(x => x.PaymentInstrumentAccountId == uniqueAccountId).Select(x => new PIMSModel.PaymentInstrument { PaymentInstrumentId = x.PaymentInstrumentId }).ToList(),
                    });
                }
            }

            var getProfileTasks = new List<Task<PIMSModel.AccountProfile>>();
            var getCustomersTasks = new List<Task<Accessors.AccountService.DataModel.CustomerInfo>>();
            foreach (var transactionRequest in searchTransactionRequests)
            {
                // Call Jarvis or Account service to get profile
                var profile = this.Settings.AccountServiceAccessor.GetProfile(transactionRequest.PaymentInstrumentAccountId, GlobalConstants.ProfileTypes.Consumer, traceActivityId);
                ////Call Jarvis or Account service to get puid
                var customer = this.Settings.AccountServiceAccessor.GetCustomers(transactionRequest.PaymentInstrumentAccountId, traceActivityId);

                getProfileTasks.Add(profile);
                getCustomersTasks.Add(customer);
            }

            try
            {
                await Task.WhenAll(getCustomersTasks);
                await Task.WhenAll(getProfileTasks);
            }
            catch (Exception ex)
            {
                SllWebLogger.TracePXServiceException($"{Operations.SearchTransactions} - Jarvis:GetProfile or Jarvis:GetCustomers failed / returned an exception " + ex, traceActivityId);
            }

            foreach (var transactionRequest in searchTransactionRequests)
            {
                var email = string.Empty;
                string puid = string.Empty;

                var customer = getCustomersTasks.Where(x => x?.Status == TaskStatus.RanToCompletion && x.Result?.Id == transactionRequest.PaymentInstrumentAccountId).Select(x => x?.Result).FirstOrDefault();
                puid = customer?.Identity?.Data?.PUID;

                var profile = getProfileTasks.Where(x => x?.Status == TaskStatus.RanToCompletion && x.Result?.AccountId == transactionRequest.PaymentInstrumentAccountId).Select(x => x?.Result).FirstOrDefault();
                if (accountId != transactionRequest.PaymentInstrumentAccountId && profile != null && profile?.EmailAddress != null)
                {
                    email = JsonDataMasker.DelegateMaskEmail(JToken.Parse(Newtonsoft.Json.JsonConvert.SerializeObject(profile?.EmailAddress))).ToString();
                }
                else
                {
                    email = profile?.EmailAddress;
                    loggedInUserEmail = email;
                }

                searchTransactionRequests
                    .Where(x => x.PaymentInstrumentAccountId == transactionRequest.PaymentInstrumentAccountId)
                    .ToList()
                    .ForEach(x =>
                    {
                        x.Email = email;
                        x.Puid = puid;
                    });
            }

            var searchTransactionTasks = searchTransactionRequests.Select(async accountinfo =>
            {
                var paymentInstrumentId = accountinfo?.PaymentInstruments.Select(x => x.PaymentInstrumentId);
                PaymentTransactions accountLevelTransaction = new PaymentTransactions();
                accountLevelTransaction.PageSize = PageSizeTransactions;

                if (!string.IsNullOrWhiteSpace(accountinfo?.Puid))
                {
                    accountLevelTransaction = await this.ListTransactionsFromCheckPI(
                        accountinfo.PaymentInstrumentAccountId,
                        null, null, 0,
                        language, partner, country,
                        accountinfo.Puid,
                        accountinfo.PaymentInstruments,
                        Operations.SearchTransactions);

                    if (accountLevelTransaction.Orders != null)
                    {
                        var filteredOrders = accountLevelTransaction.Orders
                            .Where(x => paymentInstrumentId.Contains(x.Piid))
                            .ToList();

                        foreach (var order in filteredOrders)
                        {
                            if (order.UserId == accountinfo.Puid)
                            {
                                order.Email = accountinfo.Email;
                            }
                        }

                        accountLevelTransaction.Orders = filteredOrders;
                    }
                }

                return accountLevelTransaction;
            });

            var transactionResponseList = await Task.WhenAll(searchTransactionTasks);

            foreach (var transactionResponse in transactionResponseList)
            {
                if (transactions == null)
                {
                    transactions = transactionResponse;
                    transactions.PageSize = PageSizeTransactions;
                }
                else
                {
                    transactions?.Orders.AddRange(transactionResponse.Orders);
                }
            }

            transactions.Email = loggedInUserEmail;

            return transactions;
        }
    }
}