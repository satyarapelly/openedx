// <copyright file="TemplateHelper.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PidlFactory.V7
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.Commerce.Payments.PartnerSettingsModel;
    using Microsoft.Commerce.Payments.PidlFactory.V7.PIDLGeneration;

    /// <summary>
    /// This class contains helper functions to suport PIDL templates
    /// </summary>
    public class TemplateHelper
    {
        // List of template name except the default template.
        private static List<string> templateList = new List<string>() { Constants.TemplateName.OnePage, Constants.TemplateName.TwoPage, Constants.TemplateName.SelectPMButtonList, Constants.TemplateName.SelectPMRadioButtonList, Constants.TemplateName.SelectPMDropDown, Constants.TemplateName.ListPiDropDown, Constants.TemplateName.ListPiRadioButton, Constants.TemplateName.ListPiButtonList };

        private static List<string> defaultTemplateList = new List<string>() { Constants.TemplateName.DefaultTemplate };

        private static List<string> TemplateListWithDefaultTemplate
        {
            get { return templateList.Concat(defaultTemplateList).Select(t => t.ToLowerInvariant()).ToList(); }
        }

        public static bool IsTemplateBasedPIDLIncludingDefaultTemplate(string templateOrPartnerName)
        {
            return TemplateListWithDefaultTemplate.Contains(templateOrPartnerName, StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Get default partner or template - return partner name if PSS not configured
        /// </summary>
        /// <param name="templateOrPartnerName">Partner or template name</param>
        /// <returns>Partner or template</returns>
        public static string GetDefaultTemplateOrPartner(string templateOrPartnerName)
        {
            // If display sequence not found in template which is configured in PSS then use default template.
            if (templateList.Contains(templateOrPartnerName, StringComparer.OrdinalIgnoreCase))
            {
                return Constants.TemplateName.DefaultTemplate;
            }
            else if (!string.Equals(templateOrPartnerName, Constants.PidlConfig.DefaultPartnerName, StringComparison.OrdinalIgnoreCase))
            {
                // Partners which is not using PSS.
                return Constants.PidlConfig.DefaultPartnerName;
            }

            return templateOrPartnerName;
        }

        public static bool IsListPiTemplate(PaymentExperienceSetting setting)
        {
            return setting != null
                && (string.Equals(setting.Template, Constants.TemplateName.ListPiDropDown, StringComparison.OrdinalIgnoreCase)
                || string.Equals(setting.Template, Constants.TemplateName.ListPiRadioButton, StringComparison.OrdinalIgnoreCase)
                || string.Equals(setting.Template, Constants.TemplateName.ListPiButtonList, StringComparison.OrdinalIgnoreCase));
        }

        public static bool IsTemplateBasedPIDL(string templateOrPartnerName)
        {
            return templateList.Contains(templateOrPartnerName, StringComparer.OrdinalIgnoreCase);
        }

        public static bool IsListPiButtonListTemplate(PaymentExperienceSetting setting)
        {
            return string.Equals(setting?.Template, Constants.TemplateName.ListPiButtonList, StringComparison.OrdinalIgnoreCase);
        }

        public static bool IsListPiRadioButtonTemplate(PaymentExperienceSetting setting)
        {
            return string.Equals(setting?.Template, Constants.TemplateName.ListPiRadioButton, StringComparison.OrdinalIgnoreCase);
        }

        public static bool IsSelectPMDropDownTemplate(PaymentExperienceSetting setting)
        {
            return string.Equals(setting?.Template, Constants.TemplateName.SelectPMDropDown, StringComparison.OrdinalIgnoreCase);
        }

        public static string GetSettingTemplate(string partner, PaymentExperienceSetting setting, string descriptionType, string resourceId)
        {
            string template = partner;

            if (setting?.Template != null)
            {
                template = setting.Template;

                if (setting?.Resources != null && !string.IsNullOrEmpty(descriptionType))
                {
                    Dictionary<string, ResourceSetting> resourceSettingOverrides;

                    if (!string.IsNullOrEmpty(descriptionType) && setting.Resources.TryGetValue(descriptionType, out resourceSettingOverrides))
                    {
                        ResourceSetting resourceSetting;
                        if (!string.IsNullOrEmpty(resourceId))
                        {
                            // resourceId here is {type} or {family}.{type} 
                            if (resourceSettingOverrides.TryGetValue(resourceId, out resourceSetting))
                            {
                                template = resourceSetting.Template;
                            }
                            else
                            {
                                string family = resourceId.Split('.')[0];
                                if (resourceSettingOverrides.TryGetValue(family, out resourceSetting))
                                {
                                    template = resourceSetting.Template;
                                }
                            }
                        }
                    }
                }
            }

            return template;
        }

        public static string GetRedirectionPatternFromPartnerSetting(PaymentExperienceSetting partnerSetting, string resourceType, string paymentMethodType)
        {
            string redirectionPattern = partnerSetting?.RedirectionPattern;

            if (partnerSetting?.Resources != null && !string.IsNullOrEmpty(resourceType))
            {
                Dictionary<string, ResourceSetting> resourceSettingOverrides;

                if (!string.IsNullOrEmpty(resourceType) && partnerSetting.Resources.TryGetValue(resourceType, out resourceSettingOverrides))
                {
                    ResourceSetting resourceSetting;
                    if (!string.IsNullOrEmpty(paymentMethodType))
                    {
                        // resourceId here is {type} or {family}.{type} 
                        if (resourceSettingOverrides.TryGetValue(paymentMethodType, out resourceSetting))
                        {
                            redirectionPattern = resourceSetting.RedirectionPattern;
                        }
                        else
                        {
                            string family = paymentMethodType.Split('.')[0];
                            if (resourceSettingOverrides.TryGetValue(family, out resourceSetting))
                            {
                                redirectionPattern = resourceSetting.RedirectionPattern;
                            }
                        }
                    }
                }
            }

            return redirectionPattern;
        }
    }
}