// <copyright file="ProfileDescription.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.Commerce.Payments.PidlFactory.V7;
    using Microsoft.Commerce.Payments.PidlModel.V7;
    using Microsoft.Commerce.Payments.PXCommon;

    public class ProfileDescription : ComponentDescription
    {
        private string descriptionType = V7.Constants.DescriptionTypes.ProfileDescription;
        private string descriptionId = V7.Constants.ProfileType.Checkout;

        public override string DescriptionType
        {
            get
            {
                return this.descriptionType;
            }
        }

        public override Task<List<PIDLResource>> GetDescription()
        {
            List<PIDLResource> retVal = PIDLResourceFactory.Instance.GetPaymentClientDescriptions(this.descriptionType, this.Country, this.descriptionId, this.Operation, this.Language, this.Partner, exposedFlightFeatures: this.ExposedFlightFeatures?.ToList(), scenario: this.Scenario, setting: this.PSSSetting);

            // Set default email from profile
            IDictionary<string, object> profileFields = new Dictionary<string, object>()
            {
                { V7.Constants.PropertyDescriptionIds.EmailAddress, UsePaymentRequestApiEnabled() ? this.PaymentRequestClientActions?.Profile?.Email : this.CheckoutRequestClientActions?.Profile?.Email }
            };
            ComponentDescription.SetDefaultValues(retVal, profileFields);

            // Hide submit groups and update the attach profile url with request id.
            retVal.ForEach(pidl =>
            {
                UpdateSubmitURL(pidl, V7.Constants.ButtonDisplayHintIds.SaveButton, GlobalConstants.HTTPVerbs.POST, string.Format(V7.Constants.SubmitUrls.CheckoutRequestsExAttachProfile, this.RequestId));
                var cancelBtn = pidl.GetDisplayHintById(V7.Constants.ButtonDisplayHintIds.CancelButton);
                cancelBtn.IsHidden = true;

                var saveBtn = pidl.GetDisplayHintById(V7.Constants.ButtonDisplayHintIds.SaveButton);
                saveBtn.IsHidden = true;
            });

            return Task.FromResult(retVal);
        }
    }
}