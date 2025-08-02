// <copyright file="QRCodeSecondScreenSession.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Model.PXInternal
{
    using System;
    using System.Collections.Generic;
    using System.IdentityModel.Protocols.WSTrust;
    using Microsoft.Commerce.Payments.PimsModel.V4;
    using Microsoft.Commerce.Payments.PXService.V7;
    using Microsoft.Commerce.Payments.PXService.V7.PaymentChallenge.Model;
    using Newtonsoft.Json;

    /// <summary>
    /// This is a model used by PXService internally to store and retrieve data from SessionService, primarily for add credit card qr code anonymous second screen
    /// </summary>
    public class QRCodeSecondScreenSession
    {
        public QRCodeSecondScreenSession() : base()
        {
        }

        public QRCodeSecondScreenSession(PXInternal.QRCodeSecondScreenSession context)
        {
            this.Id = context.Id;
            this.Language = context.Language;
            this.AccountId = context.AccountId;
            this.Partner = context.Partner;
            this.Country = context.Country;
            this.UseCount = context.UseCount;
            this.Operation = context.Operation;
            this.Email = context.Email;
            this.FirstName = context.FirstName;
            this.LastName = context.LastName;
            this.PaymentMethodType = context.PaymentMethodType;
            this.PaymentMethodFamily = context.PaymentMethodFamily;
            this.PaymentInstrumentId = context.PaymentInstrumentId;
            this.Status = context.Status;
            this.RiskData = context.RiskData;
            this.Signature = context.Signature;
            this.QrCodeCreatedTime = context.QrCodeCreatedTime;
            this.FormRenderedTime = context.FormRenderedTime;
            this.AllowTestHeader = context.AllowTestHeader; 
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227", Justification = "Needed to be set for serialization")]
        public List<string> ExposedFlightFeatures { get; }

        [JsonProperty(PropertyName = "AccountId")]
        public string AccountId { get; set; }

        [JsonProperty(PropertyName = "Language")]
        public string Language { get; set; }

        [JsonProperty(PropertyName = "payment_session_id")] 
        public string Id { get; set; }

        [JsonProperty(PropertyName = "Partner")]
        public string Partner { get; set; }

        [JsonProperty(PropertyName = "Country")]
        public string Country { get; set; }

        [JsonProperty(PropertyName = "UseCount")]
        public int UseCount { get; set; }

        [JsonProperty(PropertyName = "Operation")]
        public string Operation { get; set; }

        [JsonProperty(PropertyName = "Email")]
        public string Email { get; set; }

        [JsonProperty(PropertyName = "FirstName")]
        public string FirstName { get; set; }

        [JsonProperty(PropertyName = "LastName")]
        public string LastName { get; set; }

        [JsonProperty(PropertyName = "PaymentMethodType")]
        public string PaymentMethodType { get; set; }

        [JsonProperty(PropertyName = "PaymentMethodFamily")]
        public string PaymentMethodFamily { get; set; }

        [JsonProperty(PropertyName = "PaymentInstrumentId")]
        public string PaymentInstrumentId { get; set; }

        [JsonProperty(PropertyName = "Status")]
        public PaymentInstrumentStatus Status { get; set; }

        [JsonProperty(PropertyName = "RiskData")]
        public RiskData RiskData { get; set; }

        [JsonProperty(PropertyName = "signature", Required = Required.Always)]
        public string Signature { get; set; }

        [JsonProperty(PropertyName = "QrCodeCreatedTime", Required = Required.Always)]
        public DateTime QrCodeCreatedTime { get; set; }

        [JsonProperty(PropertyName = "FormRenderedTime", Required = Required.Always)]
        public DateTime FormRenderedTime { get; set; }

        // Set to true if PIFD passes the mdollarpurchase test header to identify the request as coming from a test account 
        [JsonProperty(PropertyName = "allowTestHeader")]
        public bool AllowTestHeader { get; set; }

        public string GenerateSignature()
        {
            return string.Format("placeholder_for_paymentsession_signature_{0}", this.Id);
        }
    }
}