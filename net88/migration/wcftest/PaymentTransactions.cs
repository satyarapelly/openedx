// <copyright file="PaymentTransactions.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.V7.PaymentTransaction.Model
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using Microsoft.Commerce.Payments.PimsModel.V4;
    using Microsoft.Commerce.Payments.PXService.Accessors.LegacyCommerceService.DataModel;
    using Newtonsoft.Json;
    using Catalog = PXService.Model.CatalogService;
    using D365 = PXService.Model.D365Service;
    using Purchase = PXService.Model.PurchaseService;

    public class PaymentTransactions
    {
        private static ImmutableHashSet<string> validOrderStates = ImmutableHashSet.Create(
            new string[] 
            {
                "purchased",
                "refunded",
                "canceled",
                "failed",
                "chargedback",
                "refundfailed"
            });

        public PaymentTransactions()
        {
            this.Orders = new List<Order>();
            this.Subscriptions = new List<Subscription>();
        }

        [JsonProperty("pageSize")]
        public int PageSize { get; set; }

        [JsonProperty("hasMoreRecords")]
        public bool HasMoreRecords { get; set; }

        [JsonProperty("continuationToken")]
        public string ContinuationToken { get; set; }

        [JsonProperty("orders")]
        [SuppressMessage("Microsoft.Usage", "CA2227", Justification = "Needed to be set.")]
        public List<Order> Orders { get; set; }

        [JsonProperty("subscriptions")]
        [SuppressMessage("Microsoft.Usage", "CA2227", Justification = "Needed to be set.")]
        public List<Subscription> Subscriptions { get; set; }

        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("paymentInstrument")]
        [SuppressMessage("Microsoft.Usage", "CA2227", Justification = "Needed to be set.")]
        public PaymentInstrument PaymentInstrument { get; set; }

        public static IImmutableSet<string> GetValidOrderStates()
        {
            return validOrderStates;
        }

        public void PopulateSubscriptions(List<Purchase.Subscription> purchaseSubscriptions)
        {
            if (purchaseSubscriptions != null)
            {
                purchaseSubscriptions.ForEach(sub => this.Subscriptions.Add(new Subscription(sub)));
            }
        }

        public void PopulateCtpSubscriptions(List<SubscriptionsInfo> ctpSubscriptions)
        {
            if (ctpSubscriptions != null)
            {
                foreach (var sub in ctpSubscriptions)
                {
                    var subToAdd = new Subscription(sub);
                    if (!this.Subscriptions.Any(s => s.SubscriptionId == subToAdd.SubscriptionId))
                    {
                        subToAdd.IsBlockingPi = subToAdd.RecurrenceState == "Active";
                        this.Subscriptions.Add(subToAdd);
                    }
                }
            }
        }

        public void PopulateOrders(List<Purchase.Order> purchaseOrders)
        {
            if (purchaseOrders != null)
            {
                purchaseOrders.ForEach(purchaseOrder =>
                {
                    if (purchaseOrder.OrderState != null && validOrderStates.Contains(purchaseOrder.OrderState.ToLower()))
                    {
                        Order order = new Order(purchaseOrder);
                        if (!string.IsNullOrWhiteSpace(order.Piid))
                        {
                            Orders.Add(order);
                        }
                    }
                });
            }
        }

        public void PopulatePurchaseOrder(Purchase.Order purchaseOrder)
        {
            Order order = new Order(purchaseOrder);
            if (!string.IsNullOrWhiteSpace(order.Piid))
            {
                this.Orders.Add(order);
            }
        }

        public void PopulateD365Order(D365.Order d365Order)
        {
            if (d365Order != null)
            {
                Order order = new Order(d365Order);
                if (!string.IsNullOrWhiteSpace(order.Piid))
                {
                    this.Orders.Add(order);
                }
            }
        }

        public List<string> GetProductIds()
        {
            var productIds = new List<string>();

            this.Subscriptions.ForEach(sub => productIds.Add(sub.ProductId));
            this.Orders.ForEach(order =>
            {
                if (order.OrderLineItems != null)
                {
                    order.OrderLineItems.ForEach(lineItem => productIds.Add(lineItem.ProductId));
                }
            });

            return productIds.Distinct().ToList();
        }

        public void PopulateProductNames(Catalog.Catalog catalog)
        {
            Dictionary<string, Catalog.Product> productsMap = new Dictionary<string, Catalog.Product>();
            catalog.Products.ForEach((p) => productsMap.Add(p.ProductId, p));

            this.Orders.ForEach((o) => 
            {
                if ((o.OrderLineItems?.Count ?? 0) > 0)
                {
                    // TODO: This seems like a bug.  Check with M$ if OrderLineItems can contain more than one item
                    // and if so, confirm with PM that its ok for us to show product title of just OrderLineItems[0]
                    var productId = o.OrderLineItems[0].ProductId;
                    Catalog.Product product; 

                    if (productsMap.TryGetValue(productId, out product))
                    {
                        if (product?.LocalizedProperties != null && product?.LocalizedProperties.Count > 0)
                        {
                            // TODO: This seems like a bug.  Check with Catalog team to see if its ok that we are
                            // using just LocalizedProperties[0].
                            o.Description = product.LocalizedProperties[0].ProductTitle ?? o.Description;
                        }
                    }
                }
            });

            this.Subscriptions.ForEach((s) =>
            {
                // TODO: This seems like a bug.  Is s.ProductId gauranteed to exist in the dictionary?  Just a few
                // lines above, TryGetValue was used.  Its safe to do the same here as well.
                var product = productsMap[s.ProductId];
                if (product?.LocalizedProperties != null && product?.LocalizedProperties.Count > 0)
                {
                    // TODO: Same comment as a few lines above (why just LocalizedProperties[0]?)
                    s.Title = product.LocalizedProperties[0].ProductTitle ?? s.Title;
                }
            });
        }

        public void PopulateBlockingPiResultForOrders(Dictionary<string, bool> orderIdToPaymentInUse)
        {
            foreach (var order in this.Orders)
            {
                string orderId = order.OrderId;

                if (orderIdToPaymentInUse.ContainsKey(orderId))
                {
                    order.CheckPiResult = orderIdToPaymentInUse[orderId];
                }
                else
                {
                    order.CheckPiResult = false;
                }
            }
        }

        public void PopulateBlockingPiResultForSubs(HashSet<string> blockingPiSubIds)
        {
            foreach (var sub in this.Subscriptions)
            {
                sub.IsBlockingPi = blockingPiSubIds.Contains(sub.SubscriptionId);
            }
        }
    }
}