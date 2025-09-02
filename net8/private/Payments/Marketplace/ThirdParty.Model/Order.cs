using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace ThirdParty.Model
{
    public enum OrderState
    {
        // Should never be in this state
        [EnumMember(Value = "Invalid")]
        Invalid = 0,

        [EnumMember(Value = "InCart")]
        InCart,

        [EnumMember(Value = "InCheckout")]
        InCheckOut,

        [EnumMember(Value = "PaymentExperienceCompleted")]
        PaymentExperienceCompleted,

        [EnumMember(Value = "PaymentSucceeded")]
        PaymentSucceeded,

        [EnumMember(Value = "PaymentFailed")]
        PaymentFailed,

        [EnumMember(Value = "Fulfilled")]
        Fulfilled
    }

    public class Order : BaseItem
    {
        private OrderState _state;

        public Order()
        {
            this.Items = new List<OrderLineItem>();
            this.State = OrderState.InCart;
        }

        //[DataType(DataType.Currency)]
        [DisplayFormat(DataFormatString = "{0:C0}")]
        [JsonProperty(PropertyName = "total")]
        public double Total
        {
            get
            {
                var total = 0.0d;
                if (this.Items != null)
                {                    
                    foreach(var item in this.Items)
                    {
                        total += item.Price;
                    }
                }

                return total;
            }
        }

        [JsonProperty(PropertyName = "cartDate")]
        public DateTime CartData { get; private set; }

        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:MM/dd/yyyy HH:mm}")]
        [JsonProperty(PropertyName = "checkoutDate")]
        public DateTime CheckoutDate { get; private set; }

        [JsonProperty(PropertyName = "paymentDate")]
        public DateTime PaymentDate { get; private set; }

        [JsonConverter(typeof(StringEnumConverter))]
        [JsonProperty(PropertyName = "state")]
        public OrderState State 
        {
            get => _state;
            set
            {
                _state = value;

                if (value == OrderState.InCart)
                {
                    CartData = DateTime.UtcNow;
                }
                else if (value == OrderState.InCheckOut)
                {
                    CheckoutDate = DateTime.UtcNow;
                }
                else if (value == OrderState.PaymentSucceeded)
                {
                    PaymentDate = DateTime.UtcNow;
                }
            }
        }

        [JsonProperty(PropertyName = "items")]
        public IList<OrderLineItem> Items { get; private set; }

        [JsonProperty(PropertyName = "customer")]
        public Customer Customer { get; set; }

        [JsonProperty(PropertyName = "paymentSessionId")]
        public string PaymentSessionId { get; set; }

        [JsonProperty(PropertyName = "paymentIntentId")]
        public string PaymentIntentId { get; set; }

        [JsonProperty(PropertyName = "_etag")]
        public string Etag { get; set; }

        [JsonProperty(PropertyName = "paymentId")]
        public string PaymentId { get; set; }

        [JsonProperty(PropertyName = "transactionId")]
        public string TransactionId { get; set; }

        [JsonProperty(PropertyName = "accountId")]
        public string AccountId { get; set; }

        [JsonProperty(PropertyName = "invoicePdf")]
        public string InvoicePdf { get; set; }

        [JsonProperty(PropertyName = "invoiceId")]
        public string InvoiceId { get; set; }
    }
}
