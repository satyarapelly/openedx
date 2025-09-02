// <copyright file="PropertyDisplayHintFactory.cs" company="Microsoft">Copyright (c) Microsoft 2015. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PidlFactory.V7
{
    using System;
    using System.Collections.Generic;
    using Microsoft.Commerce.Payments.PidlModel;
    using Microsoft.Commerce.Payments.PidlModel.V7;
    using Common = Microsoft.Commerce.Payments.PXCommon;

    /// <summary>
    /// This class represents a factory to generate a Property Hint by analyzing each row of the PropertyDisplayHints.csv
    /// </summary>
    public class PropertyDisplayHintFactory
    {
        private string[] propertyCells;
        private string configFileName;
        private long configFileLineNumber;

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyDisplayHintFactory"/> class
        /// </summary>
        /// <param name="cells">The cells that contains the information on the properties</param>
        /// <param name="fileName">The file name that is currently being parsed</param>
        /// <param name="lineNumber">The line number of the config file which it is currently processing for display description elements</param>
        public PropertyDisplayHintFactory(string[] cells, string fileName, long lineNumber)
        {
            if (cells == null)
            {
                throw new ArgumentNullException("cells");
            }

            if (fileName == null)
            {
                throw new ArgumentNullException("fileName");
            }

            if (string.IsNullOrEmpty(fileName))
            {
                throw new PIDLException("Argument fileName is null or empty in PropertyDisplayHintFactory ctor", Constants.ErrorCodes.PIDLArugumentIsNullOrEmpty);
            }

            this.configFileLineNumber = lineNumber;
            this.configFileName = fileName;
            this.propertyCells = cells;
            this.ValidateDisplayType();
        }

        /// <summary>
        /// Creates an instance of <see cref="DisplayHint"/>from Template
        /// </summary>
        /// <param name="partnerName">The name of the partner for which the display hint needs to be retrieved</param>
        /// <param name="operation">The operation type</param>
        /// <param name="template">The DisplayHint template</param>
        /// <param name="context">The context needed for the display hints</param>
        /// <param name="flightNames">The flight ids list for the display hints</param>
        /// <returns>Instance of a <see cref="DisplayHint"/></returns>
        public static DisplayHint CreateDisplayHintFromTemplate(string partnerName, string operation, DisplayHint template, Dictionary<string, string> context, List<string> flightNames = null)
        {
            DisplayHint newDisplayHint = null;

            PropertyDisplayHint propertyHint = template as PropertyDisplayHint;
            if (propertyHint != null)
            {
                newDisplayHint = new PropertyDisplayHint(propertyHint, context);
            }

            SecurePropertyDisplayHint securePropertyHint = template as SecurePropertyDisplayHint;
            if (securePropertyHint != null)
            {
                newDisplayHint = new SecurePropertyDisplayHint(securePropertyHint, context);
            }

            HeadingDisplayHint headingHint = template as HeadingDisplayHint;
            if (headingHint != null)
            {
                newDisplayHint = new HeadingDisplayHint(headingHint, context);
            }

            TitleDisplayHint titleHint = template as TitleDisplayHint;
            if (titleHint != null)
            {
                newDisplayHint = new TitleDisplayHint(titleHint, context);
            }

            SubheadingDisplayHint subheadingHint = template as SubheadingDisplayHint;
            if (subheadingHint != null)
            {
                newDisplayHint = new SubheadingDisplayHint(subheadingHint, context);
            }

            ButtonDisplayHint buttonHint = template as ButtonDisplayHint;
            if (buttonHint != null)
            {
                newDisplayHint = new ButtonDisplayHint(buttonHint, context);
            }

            HyperlinkDisplayHint hyperLinkHint = template as HyperlinkDisplayHint;
            if (hyperLinkHint != null)
            {
                newDisplayHint = new HyperlinkDisplayHint(hyperLinkHint, context);
            }

            WebViewDisplayHint webviewHint = template as WebViewDisplayHint;
            if (webviewHint != null)
            {
                newDisplayHint = new WebViewDisplayHint(webviewHint, context);
            }

            ImageDisplayHint imageHint = template as ImageDisplayHint;
            if (imageHint != null)
            {
                newDisplayHint = new ImageDisplayHint(imageHint, context);
            }

            LogoDisplayHint logoHint = template as LogoDisplayHint;
            if (logoHint != null)
            {
                newDisplayHint = new LogoDisplayHint(logoHint, context);
            }

            TextDisplayHint textHint = template as TextDisplayHint;
            if (textHint != null)
            {
                newDisplayHint = new TextDisplayHint(textHint, context);
            }

            SeparatorDisplayHint separatorDisplayHint = template as SeparatorDisplayHint;
            if (separatorDisplayHint != null)
            {
                newDisplayHint = new SeparatorDisplayHint(separatorDisplayHint, context);
            }

            SpinnerDisplayHint spinnerDisplayHint = template as SpinnerDisplayHint;
            if (spinnerDisplayHint != null)
            {
                newDisplayHint = new SpinnerDisplayHint(spinnerDisplayHint, context);
            }

            ExpressionDisplayHint expressionHint = template as ExpressionDisplayHint;
            if (expressionHint != null)
            {
                newDisplayHint = new ExpressionDisplayHint(expressionHint, context);
            }

            PidlContainerDisplayHint pidlContainerHint = template as PidlContainerDisplayHint;
            if (pidlContainerHint != null)
            {
                newDisplayHint = new PidlContainerDisplayHint(pidlContainerHint, context);
            }

            PrefillControlDisplayHint prefillHint = template as PrefillControlDisplayHint;
            if (prefillHint != null)
            {
                newDisplayHint = new PrefillControlDisplayHint(prefillHint, context);
            }

            ExpressCheckoutButtonDisplayHint expressCheckoutButtonPropertyHint = template as ExpressCheckoutButtonDisplayHint;
            if (expressCheckoutButtonPropertyHint != null)
            {
                newDisplayHint = new ExpressCheckoutButtonDisplayHint(expressCheckoutButtonPropertyHint, context);
            }

            if (newDisplayHint != null)
            {
                string currentCountry = Context.Country;

                if (string.IsNullOrEmpty(currentCountry))
                {
                    currentCountry = GlobalConstants.Defaults.CountryKey;
                }

                if (!string.IsNullOrWhiteSpace(newDisplayHint.DisplayHelpSequenceId))
                {
                    newDisplayHint.HelpDisplayDescriptions = PIDLResourceDisplayHintFactory.Instance.GetDisplayHints(
                        partnerName,
                        newDisplayHint.DisplayHelpSequenceId,
                        currentCountry,
                        operation,
                        context,
                        null,
                        flightNames);
                }

                PropertyDisplayHint newPropertyDisplayHint = newDisplayHint as PropertyDisplayHint;
                if (newPropertyDisplayHint != null)
                {
                    PropertyDisplayErrorMessageMap propertyMessageCodeMap = PIDLResourceDisplayHintFactory.Instance.GetPropertyDisplayErrorMessages(
                    newPropertyDisplayHint.HintId,
                    currentCountry,
                    partnerName);

                    if (propertyMessageCodeMap != null)
                    {
                        newPropertyDisplayHint.DisplayErrorMessages = new PropertyDisplayErrorMessageMap(propertyMessageCodeMap);
                    }
                }

                SecurePropertyDisplayHint newSecurePropertyDisplayHint = newDisplayHint as SecurePropertyDisplayHint;
                if (newSecurePropertyDisplayHint != null)
                {
                    PropertyDisplayErrorMessageMap propertyMessageCodeMap = PIDLResourceDisplayHintFactory.Instance.GetPropertyDisplayErrorMessages(
                        newSecurePropertyDisplayHint.HintId,
                        currentCountry,
                        partnerName);

                    if (propertyMessageCodeMap != null)
                    {
                        newSecurePropertyDisplayHint.DisplayErrorMessages = new PropertyDisplayErrorMessageMap(propertyMessageCodeMap);
                    }
                }
            }

            return newDisplayHint;
        }

        /// <summary>
        /// Returns the appropriate DisplayHint from the factory
        /// </summary>
        /// <param name="displayDictionaries">The DisplayDictionaries for a given partner</param>
        /// <returns>Instance of a <see cref="DisplayHint"/></returns>
        public DisplayHint CreateDisplayHint(Dictionary<string, Dictionary<string, string[]>> displayDictionaries)
        {
            string displayType = this.propertyCells[CellIndexDescription.DisplayType];

            if (string.Equals(displayType, HintType.Property.ToString(), StringComparison.InvariantCultureIgnoreCase))
            {
                return this.GetPropertyDisplayHint(displayDictionaries);
            }
            else if (string.Equals(displayType, HintType.SecureProperty.ToString(), StringComparison.InvariantCultureIgnoreCase))
            {
                return this.GetSecurePropertyDisplayHint(displayDictionaries);
            }
            else if (string.Equals(displayType, HintType.Title.ToString(), StringComparison.InvariantCultureIgnoreCase))
            {
                return this.GetTitleDisplayHint();
            }
            else if (string.Equals(displayType, HintType.Heading.ToString(), StringComparison.InvariantCultureIgnoreCase))
            {
                return this.GetHeadingDisplayHint();
            }
            else if (string.Equals(displayType, HintType.SubHeading.ToString(), StringComparison.InvariantCultureIgnoreCase))
            {
                return this.GetSubHeadingDisplayHint();
            }
            else if (string.Equals(displayType, HintType.Text.ToString(), StringComparison.InvariantCultureIgnoreCase))
            {
                return this.GetTextDisplayHint();
            }
            else if (string.Equals(displayType, HintType.Button.ToString(), StringComparison.InvariantCultureIgnoreCase))
            {
                return this.GetButtonDisplayHint();
            }
            else if (string.Equals(displayType, HintType.Hyperlink.ToString(), StringComparison.InvariantCultureIgnoreCase))
            {
                return this.GetHyperlinkDisplayHint();
            }
            else if (string.Equals(displayType, HintType.Image.ToString(), StringComparison.InvariantCultureIgnoreCase))
            {
                return this.GetImageDisplayHint();
            }
            else if (string.Equals(displayType, HintType.Logo.ToString(), StringComparison.InvariantCultureIgnoreCase))
            {
                return this.GetLogoDisplayHint();
            }
            else if (string.Equals(displayType, HintType.WebView.ToString(), StringComparison.InvariantCultureIgnoreCase))
            {
                return this.GetWebViewDisplayHint();
            }
            else if (string.Equals(displayType, HintType.Expression.ToString(), StringComparison.InvariantCultureIgnoreCase))
            {
                return this.GetExpressionDisplayHint();
            }
            else if (string.Equals(displayType, HintType.PidlContainer.ToString(), StringComparison.InvariantCultureIgnoreCase))
            {
                return this.GetPidlContainerDisplayHint();
            }
            else if (string.Equals(displayType, HintType.PrefillControl.ToString(), StringComparison.InvariantCultureIgnoreCase))
            {
                return this.GetPrefillControlDisplayHint();
            }
            else if (string.Equals(displayType, HintType.Separator.ToString(), StringComparison.InvariantCultureIgnoreCase))
            {
                return this.GetSeparatorDisplayHint();
            }
            else if (string.Equals(displayType, HintType.Spinner.ToString(), StringComparison.InvariantCultureIgnoreCase))
            {
                return this.GetSpinnerDisplayHint();
            }
            else if (string.Equals(displayType, HintType.ExpressCheckoutButton.ToString(), StringComparison.InvariantCultureIgnoreCase))
            {
                return this.GetExpressCheckoutButtonDisplayHint(displayDictionaries);
            }
            else
            {
                return null;
            }
        }

        private static void ValidateDisplayHintActionType(string clientActionType)
        {
            if (string.IsNullOrEmpty(clientActionType))
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(clientActionType))
            {
                return;
            }

            DisplayHintActionType actionType;
            if (!Enum.TryParse<DisplayHintActionType>(clientActionType, out actionType))
            {
                throw new PIDLException(
                    string.Format("The ClientAction has unexpected value : {0}", clientActionType),
                    Constants.ErrorCodes.PIDLArgumentTypeIsInvalid);
            }
        }

        private static string ConstructImageSourceUrl(string url)
        {
            string sourceUrl = null;

            if (url != null)
            {
                if ((url.StartsWith("{$.", StringComparison.OrdinalIgnoreCase) && url.EndsWith("}", StringComparison.OrdinalIgnoreCase)) || (url.StartsWith("(", StringComparison.OrdinalIgnoreCase) && url.EndsWith(")", StringComparison.OrdinalIgnoreCase)) || url.StartsWith("https://", StringComparison.OrdinalIgnoreCase) || url.StartsWith("http://", StringComparison.OrdinalIgnoreCase) /* lgtm[cs/non-https-url] Suppressing Semmle warning*/)
                {
                    sourceUrl = url;
                }
                else
                {
                    sourceUrl = (Microsoft.Commerce.Payments.Common.Environments.Environment.IsProdOrPPEEnvironment ? "https://pmservices.cp.microsoft.com" : "https://pmservices.cp.microsoft-int.com") + url;
                }
            }

            return sourceUrl;
        }

        private WebViewDisplayHint GetWebViewDisplayHint()
        {
            string sourceUrl = this.propertyCells[CellIndexDescription.SourceUrl];

            if (string.IsNullOrEmpty(sourceUrl))
            {
                throw new PIDLConfigException(
                    this.configFileName,
                    this.configFileLineNumber,
                    "SourceUrl is a mandatory field for WebView",
                    Constants.ErrorCodes.PIDLConfigFileRequiredColumnIsMissing);
            }

            WebViewDisplayHint newDisplayHint = new WebViewDisplayHint()
            {
                HintId = this.propertyCells[CellIndexDescription.PropertyHintId],
                IsHidden = this.GetIsHiddenAttributeValue(),
                IsDisabled = this.GetIsDisabledAttributeValue(),
                IsHighlighted = this.GetIsHighlightedAttributeValue(),
                IsBack = this.GetIsBackAttributeValue(),
                SourceUrl = string.IsNullOrEmpty(this.propertyCells[CellIndexDescription.SourceUrl]) ? null : this.propertyCells[CellIndexDescription.SourceUrl],
                DisplayCondition = this.GetDisplayConditionAttributeValue(),
                StyleHints = this.GetStyleHintsAttributeValue()
            };

            return newDisplayHint;
        }

        private ImageDisplayHint GetImageDisplayHint()
        {
            string imageSource = this.propertyCells[CellIndexDescription.SourceUrl];

            if (string.IsNullOrEmpty(imageSource))
            {
                throw new PIDLConfigException(
                    this.configFileName,
                    this.configFileLineNumber,
                    "SourceUrl is a mandatory field for Image",
                    Constants.ErrorCodes.PIDLConfigFileRequiredColumnIsMissing);
            }

            // if sourceURL contains FontIcon, then extract the codepoint and make ImageDisplayHint without a sourceURL
            // ex SourceUrl: FontIcon:0xE10F
            if (imageSource.StartsWith("FontIcon:"))
            {
                string[] parts = imageSource.Split(':');
                if (parts.Length != 2 || string.IsNullOrEmpty(parts[1]))
                {
                    throw new PIDLConfigException(
                        this.configFileName,
                        this.configFileLineNumber,
                        "Invalid FontIcon format in SourceUrl",
                        Constants.ErrorCodes.PIDLConfigInvalidSourceUrlFormat);
                }

                string codepoint = parts[1];
                return new ImageDisplayHint()
                {
                    HintId = this.propertyCells[CellIndexDescription.PropertyHintId],
                    IsHidden = this.GetIsHiddenAttributeValue(),
                    IsDisabled = this.GetIsDisabledAttributeValue(),
                    IsHighlighted = this.GetIsHighlightedAttributeValue(),
                    IsBack = this.GetIsBackAttributeValue(),
                    DisplayContent = string.IsNullOrEmpty(this.propertyCells[CellIndexDescription.DisplayContent]) ? null : this.propertyCells[CellIndexDescription.DisplayContent],
                    Codepoint = codepoint,
                    DisplayCondition = this.GetDisplayConditionAttributeValue(),
                    StyleHints = this.GetStyleHintsAttributeValue()
                };
            }

            ImageDisplayHint newDisplayHint = new ImageDisplayHint()
            {
                HintId = this.propertyCells[CellIndexDescription.PropertyHintId],
                IsHidden = this.GetIsHiddenAttributeValue(),
                IsDisabled = this.GetIsDisabledAttributeValue(),
                IsHighlighted = this.GetIsHighlightedAttributeValue(),
                IsBack = this.GetIsBackAttributeValue(),
                DisplayContent = string.IsNullOrEmpty(this.propertyCells[CellIndexDescription.DisplayContent]) ? null : this.propertyCells[CellIndexDescription.DisplayContent],
                SourceUrl = ConstructImageSourceUrl(imageSource),
                DisplayCondition = this.GetDisplayConditionAttributeValue(),
                StyleHints = this.GetStyleHintsAttributeValue()
            };

            return newDisplayHint;
        }

        private SeparatorDisplayHint GetSeparatorDisplayHint()
        {
            SeparatorDisplayHint newDisplayHint = new SeparatorDisplayHint()
            {
                HintId = this.propertyCells[CellIndexDescription.PropertyHintId],
                IsHidden = this.GetIsHiddenAttributeValue(),
                IsDisabled = this.GetIsDisabledAttributeValue(),
                IsHighlighted = this.GetIsHighlightedAttributeValue(),
                IsBack = this.GetIsBackAttributeValue(),
                StyleHints = this.GetStyleHintsAttributeValue()
            };

            return newDisplayHint;
        }

        private SpinnerDisplayHint GetSpinnerDisplayHint()
        {
            SpinnerDisplayHint newDisplayHint = new SpinnerDisplayHint()
            {
                HintId = this.propertyCells[CellIndexDescription.PropertyHintId],
                IsHidden = this.GetIsHiddenAttributeValue(),
                IsDisabled = this.GetIsDisabledAttributeValue(),
                IsHighlighted = this.GetIsHighlightedAttributeValue(),
                IsBack = this.GetIsBackAttributeValue(),
            };

            return newDisplayHint;
        }

        private LogoDisplayHint GetLogoDisplayHint()
        {
            string imageSource = this.propertyCells[CellIndexDescription.SourceUrl];

            if (string.IsNullOrEmpty(imageSource))
            {
                throw new PIDLConfigException(
                    this.configFileName,
                    this.configFileLineNumber,
                    "SourceUrl is a mandatory field for Logo",
                    Constants.ErrorCodes.PIDLConfigFileRequiredColumnIsMissing);
            }

            LogoDisplayHint newDisplayHint = new LogoDisplayHint()
            {
                HintId = this.propertyCells[CellIndexDescription.PropertyHintId],
                IsHidden = this.GetIsHiddenAttributeValue(),
                IsDisabled = this.GetIsDisabledAttributeValue(),
                IsHighlighted = this.GetIsHighlightedAttributeValue(),
                IsBack = this.GetIsBackAttributeValue(),
                DisplayHelpSequenceText = this.GetNullableStringAttributeValue(CellIndexDescription.DisplayHelpSequenceText),
                SourceUrl = string.IsNullOrEmpty(this.propertyCells[CellIndexDescription.SourceUrl]) ? null : (Microsoft.Commerce.Payments.Common.Environments.Environment.IsProdOrPPEEnvironment ? "https://pmservices.cp.microsoft.com" : "https://pmservices.cp.microsoft-int.com") + this.propertyCells[CellIndexDescription.SourceUrl],
                DisplayCondition = this.GetDisplayConditionAttributeValue(),
                StyleHints = this.GetStyleHintsAttributeValue()
            };

            return newDisplayHint;
        }

        private ExpressionDisplayHint GetExpressionDisplayHint()
        {
            string displayContent = this.propertyCells[CellIndexDescription.DisplayContent];

            if (string.IsNullOrEmpty(displayContent))
            {
                throw new PIDLConfigException(
                    this.configFileName,
                    this.configFileLineNumber,
                    "DisplayContent is a mandatory field for Text",
                    Constants.ErrorCodes.PIDLConfigFileRequiredColumnIsMissing);
            }

            ExpressionDisplayHint newDisplayHint = new ExpressionDisplayHint()
            {
                HintId = this.propertyCells[CellIndexDescription.PropertyHintId],
                IsHidden = this.GetIsHiddenAttributeValue(),
                IsDisabled = this.GetIsDisabledAttributeValue(),
                IsHighlighted = this.GetIsHighlightedAttributeValue(),
                IsBack = this.GetIsBackAttributeValue(),
                DisplayContent = this.propertyCells[CellIndexDescription.DisplayContent],
                DisplayCondition = this.GetDisplayConditionAttributeValue(),
                StyleHints = this.GetStyleHintsAttributeValue()
            };

            return newDisplayHint;
        }

        private TextDisplayHint GetTextDisplayHint()
        {
            string displayContent = this.propertyCells[CellIndexDescription.DisplayContent];

            if (string.IsNullOrEmpty(displayContent))
            {
                throw new PIDLConfigException(
                    this.configFileName,
                    this.configFileLineNumber,
                    "DisplayContent is a mandatory field for Text",
                    Constants.ErrorCodes.PIDLConfigFileRequiredColumnIsMissing);
            }

            string dependentPropertyName = string.IsNullOrEmpty(this.propertyCells[CellIndexDescription.DependentPropertyName]) ? null : this.propertyCells[CellIndexDescription.DependentPropertyName];
            string dependentPropertyValueRegex = string.IsNullOrEmpty(this.propertyCells[CellIndexDescription.DependentPropertyValueRegex]) ? null : this.propertyCells[CellIndexDescription.DependentPropertyName];

            if (!string.IsNullOrEmpty(dependentPropertyName))
            {
                if (string.IsNullOrEmpty(dependentPropertyValueRegex))
                {
                    throw new PIDLConfigException(
                        this.configFileName,
                        this.configFileLineNumber,
                        "DependentPropertyValueRegex is a mandatory field if a DependentPropertyName is passed",
                        Constants.ErrorCodes.PIDLConfigFileRequiredColumnIsMissing);
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(dependentPropertyValueRegex))
                {
                    throw new PIDLConfigException(
                        this.configFileName,
                        this.configFileLineNumber,
                        "DependentPropertyName is a mandatory field if a DependentPropertyValueRegex is passed",
                        Constants.ErrorCodes.PIDLConfigFileRequiredColumnIsMissing);
                }
            }

            TextDisplayHint newDisplayHint = new TextDisplayHint()
            {
                HintId = this.propertyCells[CellIndexDescription.PropertyHintId],
                IsHidden = this.GetIsHiddenAttributeValue(),
                IsDisabled = this.GetIsDisabledAttributeValue(),
                IsHighlighted = this.GetIsHighlightedAttributeValue(),
                IsBack = this.GetIsBackAttributeValue(),
                DisplayContent = this.propertyCells[CellIndexDescription.DisplayContent],
                DisplayContentDescription = string.IsNullOrEmpty(this.propertyCells[CellIndexDescription.DisplayContentDescription]) ? null : this.propertyCells[CellIndexDescription.DisplayContentDescription],
                PropertyName = string.IsNullOrEmpty(this.propertyCells[CellIndexDescription.PropertyName]) ? null : this.propertyCells[CellIndexDescription.PropertyName],
                DependentPropertyName = string.IsNullOrEmpty(this.propertyCells[CellIndexDescription.DependentPropertyName]) ? null : this.propertyCells[CellIndexDescription.DependentPropertyName],
                DependentPropertyValueRegex = string.IsNullOrEmpty(this.propertyCells[CellIndexDescription.DependentPropertyValueRegex]) ? null : this.propertyCells[CellIndexDescription.DependentPropertyValueRegex],
                DisplayCondition = this.GetDisplayConditionAttributeValue(),
                StyleHints = this.GetStyleHintsAttributeValue()
            };

            string clientActionType = this.propertyCells[CellIndexDescription.ClientAction];
            ValidateDisplayHintActionType(clientActionType);
            newDisplayHint.Action = string.IsNullOrEmpty(
                this.propertyCells[CellIndexDescription.ClientAction]) ?
                null : new DisplayHintAction(
                    this.propertyCells[CellIndexDescription.ClientAction],
                    this.GetIsDefaultAttributeValue(),
                    this.propertyCells[CellIndexDescription.Context],
                    this.GetNullableStringAttributeValue(CellIndexDescription.DestinationId));

            return newDisplayHint;
        }

        private SubheadingDisplayHint GetSubHeadingDisplayHint()
        {
            string displayContent = this.propertyCells[CellIndexDescription.DisplayContent];

            if (string.IsNullOrEmpty(displayContent))
            {
                throw new PIDLConfigException(
                    this.configFileName,
                    this.configFileLineNumber,
                    "DisplayContent is a mandatory field for Subheading",
                    Constants.ErrorCodes.PIDLConfigFileRequiredColumnIsMissing);
            }

            SubheadingDisplayHint newDisplayHint = new SubheadingDisplayHint()
            {
                HintId = this.propertyCells[CellIndexDescription.PropertyHintId],
                IsHidden = this.GetIsHiddenAttributeValue(),
                IsDisabled = this.GetIsDisabledAttributeValue(),
                IsHighlighted = this.GetIsHighlightedAttributeValue(),
                IsBack = this.GetIsBackAttributeValue(),
                DisplayContent = this.propertyCells[CellIndexDescription.DisplayContent],
                DisplayContentDescription = string.IsNullOrEmpty(this.propertyCells[CellIndexDescription.DisplayContentDescription]) ? null : this.propertyCells[CellIndexDescription.DisplayContentDescription],
                DisplayCondition = this.GetDisplayConditionAttributeValue(),
                StyleHints = this.GetStyleHintsAttributeValue()
            };

            return newDisplayHint;
        }

        private TitleDisplayHint GetTitleDisplayHint()
        {
            string displayContent = this.propertyCells[CellIndexDescription.DisplayContent];

            if (string.IsNullOrEmpty(displayContent))
            {
                throw new PIDLConfigException(
                    this.configFileName,
                    this.configFileLineNumber,
                    "DisplayContent is a mandatory field for Title",
                    Constants.ErrorCodes.PIDLConfigFileRequiredColumnIsMissing);
            }

            TitleDisplayHint newDisplayHint = new TitleDisplayHint()
            {
                HintId = this.propertyCells[CellIndexDescription.PropertyHintId],
                IsHidden = this.GetIsHiddenAttributeValue(),
                IsDisabled = this.GetIsDisabledAttributeValue(),
                IsHighlighted = this.GetIsHighlightedAttributeValue(),
                IsBack = this.GetIsBackAttributeValue(),
                DisplayContent = this.propertyCells[CellIndexDescription.DisplayContent],
                DisplayContentDescription = string.IsNullOrEmpty(this.propertyCells[CellIndexDescription.DisplayContentDescription]) ? null : this.propertyCells[CellIndexDescription.DisplayContentDescription],
                DisplayCondition = this.GetDisplayConditionAttributeValue(),
                StyleHints = this.GetStyleHintsAttributeValue()
            };

            return newDisplayHint;
        }

        private HeadingDisplayHint GetHeadingDisplayHint()
        {
            string displayContent = this.propertyCells[CellIndexDescription.DisplayContent];

            if (string.IsNullOrEmpty(displayContent))
            {
                throw new PIDLConfigException(
                    this.configFileName,
                    this.configFileLineNumber,
                    "DisplayContent is a mandatory field for Heading",
                    Constants.ErrorCodes.PIDLConfigFileRequiredColumnIsMissing);
            }

            HeadingDisplayHint newDisplayHint = new HeadingDisplayHint()
            {
                HintId = this.propertyCells[CellIndexDescription.PropertyHintId],
                IsHidden = this.GetIsHiddenAttributeValue(),
                IsDisabled = this.GetIsDisabledAttributeValue(),
                IsHighlighted = this.GetIsHighlightedAttributeValue(),
                IsBack = this.GetIsBackAttributeValue(),
                DisplayContent = this.propertyCells[CellIndexDescription.DisplayContent],
                DisplayContentDescription = string.IsNullOrEmpty(this.propertyCells[CellIndexDescription.DisplayContentDescription]) ? null : this.propertyCells[CellIndexDescription.DisplayContentDescription],
                DisplayCondition = this.GetDisplayConditionAttributeValue(),
                StyleHints = this.GetStyleHintsAttributeValue()
            };

            return newDisplayHint;
        }

        private PidlContainerDisplayHint GetPidlContainerDisplayHint()
        {
            return new PidlContainerDisplayHint()
            {
                HintId = this.propertyCells[CellIndexDescription.PropertyHintId],
                DisplayCondition = this.GetDisplayConditionAttributeValue(),
                StyleHints = this.GetStyleHintsAttributeValue()
            };
        }

        private PrefillControlDisplayHint GetPrefillControlDisplayHint()
        {
            return new PrefillControlDisplayHint()
            {
                HintId = this.propertyCells[CellIndexDescription.PropertyHintId],
                DisplayName = this.GetNullableStringAttributeValue(CellIndexDescription.DisplayName),
                SelectType = this.GetNullableStringAttributeValue(CellIndexDescription.SelectType),
                PropertyName = string.IsNullOrEmpty(this.propertyCells[CellIndexDescription.PropertyName]) ? null : this.propertyCells[CellIndexDescription.PropertyName],
                ShowDisplayName = this.GetDisplayNameAttributeValue(),
                DisplayCondition = this.GetDisplayConditionAttributeValue(),
                StyleHints = this.GetStyleHintsAttributeValue()
            };
        }

        private SecurePropertyDisplayHint GetSecurePropertyDisplayHint(Dictionary<string, Dictionary<string, string[]>> displayDictionaries)
        {
            string propertyName = this.propertyCells[CellIndexDescription.PropertyName];

            if (string.IsNullOrWhiteSpace(propertyName))
            {
                throw new PIDLConfigException(
                    this.configFileName,
                    this.configFileLineNumber,
                    "PropertyName is a mandatory field for Property",
                    Constants.ErrorCodes.PIDLConfigFileRequiredColumnIsMissing);
            }

            string dependentPropertyName = string.IsNullOrEmpty(this.propertyCells[CellIndexDescription.DependentPropertyName]) ? null : this.propertyCells[CellIndexDescription.DependentPropertyName];
            string dependentPropertyValueRegex = string.IsNullOrEmpty(this.propertyCells[CellIndexDescription.DependentPropertyValueRegex]) ? null : this.propertyCells[CellIndexDescription.DependentPropertyValueRegex];

            if (!string.IsNullOrWhiteSpace(dependentPropertyName))
            {
                if (string.IsNullOrWhiteSpace(dependentPropertyValueRegex))
                {
                    throw new PIDLConfigException(
                        this.configFileName,
                        this.configFileLineNumber,
                        "DependentPropertyValueRegex is a mandatory field if a DependentPropertyName is passed",
                        Constants.ErrorCodes.PIDLConfigFileRequiredColumnIsMissing);
                }
            }
            else
            {
                if (!string.IsNullOrWhiteSpace(dependentPropertyValueRegex))
                {
                    throw new PIDLConfigException(
                        this.configFileName,
                        this.configFileLineNumber,
                        "DependentPropertyName is a mandatory field if a DependentPropertyValueRegex is passed",
                        Constants.ErrorCodes.PIDLConfigFileRequiredColumnIsMissing);
                }
            }

            SecurePropertyDisplayHint newDisplayHint = new SecurePropertyDisplayHint()
            {
                HintId = this.propertyCells[CellIndexDescription.PropertyHintId],
                IsHidden = this.GetIsHiddenAttributeValue(),
                IsDisabled = this.GetIsDisabledAttributeValue(),
                IsHighlighted = this.GetIsHighlightedAttributeValue(),
                IsBack = this.GetIsBackAttributeValue(),
                MaskInput = this.GetMaskInputAttributeValue(),
                InputScope = this.GetInputScopeAttribute(),
                PropertyName = this.propertyCells[CellIndexDescription.PropertyName],
                DependentPropertyName = dependentPropertyName,
                DependentPropertyValueRegex = dependentPropertyValueRegex,
                MinLength = this.GetNullableIntegerAttributeValue(CellIndexDescription.MinSize),
                MaxLength = this.GetNullableIntegerAttributeValue(CellIndexDescription.MaxSize),
                DisplayName = this.GetNullableStringAttributeValue(CellIndexDescription.DisplayName),
                DisplayDescription = this.GetNullableStringAttributeValue(CellIndexDescription.DisplayDescription),
                DisplayImage = string.IsNullOrEmpty(this.propertyCells[CellIndexDescription.DisplayImage]) ? null : (Microsoft.Commerce.Payments.Common.Environments.Environment.IsProdOrPPEEnvironment ? "https://pmservices.cp.microsoft.com" : "https://pmservices.cp.microsoft-int.com") + this.propertyCells[CellIndexDescription.DisplayImage],
                DisplayHelpSequenceId = this.GetNullableStringAttributeValue(CellIndexDescription.DisplayHelpSequenceId),
                DisplayHelpSequenceText = this.GetNullableStringAttributeValue(CellIndexDescription.DisplayHelpSequenceText),
                SelectType = this.GetNullableStringAttributeValue(CellIndexDescription.SelectType),
                MaskDisplay = this.GetMaskDisplayAttributeValue(),
                DisplayCondition = this.GetDisplayConditionAttributeValue(),
                DisplayHelpPosition = this.GetNullableStringAttributeValue(CellIndexDescription.DisplayHelpPosition),
                DisplayLogo = string.IsNullOrEmpty(this.propertyCells[CellIndexDescription.DisplayLogo]) ? null : (Microsoft.Commerce.Payments.Common.Environments.Environment.IsProdOrPPEEnvironment ? "https://pmservices.cp.microsoft.com" : "https://pmservices.cp.microsoft-int.com") + this.propertyCells[CellIndexDescription.DisplayLogo],
            };

            newDisplayHint.SourceUrl = "https://{securePx-endpoint}/resources/securefield.html";
            newDisplayHint.FrameName = newDisplayHint.PropertyName;

            if (!string.IsNullOrWhiteSpace(this.propertyCells[CellIndexDescription.DisplaySelectionText]) && string.IsNullOrEmpty(this.propertyCells[CellIndexDescription.PossibleValues]))
            {
                throw new PIDLConfigException(
                    this.configFileName,
                    this.configFileLineNumber,
                    "DisplaySelectionText can exist only when PossibleValues exist.",
                    Constants.ErrorCodes.PIDLConfigMissingPossibleValues);
            }

            newDisplayHint.DisplaySelectionText = this.propertyCells[CellIndexDescription.DisplaySelectionText];

            var displayFormats = string.IsNullOrWhiteSpace(this.propertyCells[CellIndexDescription.DisplayFormat]) ? null : this.propertyCells[CellIndexDescription.DisplayFormat].Split(';');

            if (displayFormats != null)
            {
                foreach (var displayFormat in displayFormats)
                {
                    newDisplayHint.AddDisplayFormat(displayFormat);
                }
            }

            var displayExamples = string.IsNullOrWhiteSpace(this.propertyCells[CellIndexDescription.DisplayExample]) ? null : this.propertyCells[CellIndexDescription.DisplayExample].Split(';');

            if (displayExamples != null)
            {
                foreach (var displayExample in displayExamples)
                {
                    newDisplayHint.AddDisplayExample(displayExample);
                }
            }

            newDisplayHint.ShowDisplayName = this.GetDisplayNameAttributeValue();

            if (!string.IsNullOrWhiteSpace(this.propertyCells[CellIndexDescription.ClientAction]))
            {
                string clientActionType = this.propertyCells[CellIndexDescription.ClientAction];
                ValidateDisplayHintActionType(clientActionType);
                newDisplayHint.Action = new DisplayHintAction(
                    this.propertyCells[CellIndexDescription.ClientAction],
                    this.GetIsDefaultAttributeValue(),
                    this.propertyCells[CellIndexDescription.Context],
                    this.GetNullableStringAttributeValue(CellIndexDescription.DestinationId));
            }

            string possibleValues = this.propertyCells[PropertyDisplayHintFactory.CellIndexDescription.PossibleValues];
            Dictionary<string, string[]> possibleValuesDictionary = string.IsNullOrEmpty(possibleValues) ? null : DisplayDescriptionStore.GetDictionaryFromConfigString(possibleValues, displayDictionaries);

            return newDisplayHint;
        }

        private PropertyDisplayHint GetPropertyDisplayHint(Dictionary<string, Dictionary<string, string[]>> displayDictionaries)
        {
            string propertyName = this.propertyCells[CellIndexDescription.PropertyName];

            if (string.IsNullOrEmpty(propertyName))
            {
                throw new PIDLConfigException(
                    this.configFileName,
                    this.configFileLineNumber,
                    "PropertyName is a mandatory field for Property",
                    Constants.ErrorCodes.PIDLConfigFileRequiredColumnIsMissing);
            }

            string dependentPropertyName = string.IsNullOrEmpty(this.propertyCells[CellIndexDescription.DependentPropertyName]) ? null : this.propertyCells[CellIndexDescription.DependentPropertyName];
            string dependentPropertyValueRegex = string.IsNullOrEmpty(this.propertyCells[CellIndexDescription.DependentPropertyValueRegex]) ? null : this.propertyCells[CellIndexDescription.DependentPropertyValueRegex];

            if (!string.IsNullOrEmpty(dependentPropertyName))
            {
                if (string.IsNullOrEmpty(dependentPropertyValueRegex))
                {
                    throw new PIDLConfigException(
                        this.configFileName,
                        this.configFileLineNumber,
                        "DependentPropertyValueRegex is a mandatory field if a DependentPropertyName is passed",
                        Constants.ErrorCodes.PIDLConfigFileRequiredColumnIsMissing);
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(dependentPropertyValueRegex))
                {
                    throw new PIDLConfigException(
                        this.configFileName,
                        this.configFileLineNumber,
                        "DependentPropertyName is a mandatory field if a DependentPropertyValueRegex is passed",
                        Constants.ErrorCodes.PIDLConfigFileRequiredColumnIsMissing);
                }
            }

            PropertyDisplayHint newDisplayHint = new PropertyDisplayHint()
            {
                HintId = this.propertyCells[CellIndexDescription.PropertyHintId],
                IsHidden = this.GetIsHiddenAttributeValue(),
                IsDisabled = this.GetIsDisabledAttributeValue(),
                IsHighlighted = this.GetIsHighlightedAttributeValue(),
                IsBack = this.GetIsBackAttributeValue(),
                MaskInput = this.GetMaskInputAttributeValue(),
                InputScope = this.GetInputScopeAttribute(),
                PropertyName = this.propertyCells[CellIndexDescription.PropertyName],
                DependentPropertyName = dependentPropertyName,
                DependentPropertyValueRegex = dependentPropertyValueRegex,
                MinLength = this.GetNullableIntegerAttributeValue(CellIndexDescription.MinSize),
                MaxLength = this.GetNullableIntegerAttributeValue(CellIndexDescription.MaxSize),
                DisplayName = this.GetNullableStringAttributeValue(CellIndexDescription.DisplayName),
                DisplayDescription = this.GetNullableStringAttributeValue(CellIndexDescription.DisplayDescription),
                DisplayImage = string.IsNullOrEmpty(this.propertyCells[CellIndexDescription.DisplayImage]) ? null : (Microsoft.Commerce.Payments.Common.Environments.Environment.IsProdOrPPEEnvironment ? "https://pmservices.cp.microsoft.com" : "https://pmservices.cp.microsoft-int.com") + this.propertyCells[CellIndexDescription.DisplayImage],
                DisplayHelpSequenceId = this.GetNullableStringAttributeValue(CellIndexDescription.DisplayHelpSequenceId),
                DisplayHelpSequenceText = this.GetNullableStringAttributeValue(CellIndexDescription.DisplayHelpSequenceText),
                DisplayLogo = string.IsNullOrEmpty(this.propertyCells[CellIndexDescription.DisplayLogo]) ? null : (Microsoft.Commerce.Payments.Common.Environments.Environment.IsProdOrPPEEnvironment ? "https://pmservices.cp.microsoft.com" : "https://pmservices.cp.microsoft-int.com") + this.propertyCells[CellIndexDescription.DisplayLogo],
                SelectType = this.GetNullableStringAttributeValue(CellIndexDescription.SelectType),
                MaskDisplay = this.GetMaskDisplayAttributeValue(),
                DataCollectionSource = this.GetNullableStringAttributeValue(CellIndexDescription.DataCollectionSource),
                DataCollectionFilterDescription = this.GetDataCollectionFilterDescriptionAttributeValue(),
                DisplayCondition = this.GetDisplayConditionAttributeValue(),
                IsSelectFirstItem = this.GetIsSelectFirstItemAttributeValue(),
                DisplayHelpPosition = this.GetNullableStringAttributeValue(CellIndexDescription.DisplayHelpPosition),
                StyleHints = this.GetStyleHintsAttributeValue()
            };

            if (!string.IsNullOrEmpty(this.propertyCells[CellIndexDescription.DisplaySelectionText]) && string.IsNullOrEmpty(this.propertyCells[CellIndexDescription.PossibleValues]))
            {
                throw new PIDLConfigException(
                    this.configFileName,
                    this.configFileLineNumber,
                    "DisplaySelectionText can exist only when PossibleValues exist.",
                    Constants.ErrorCodes.PIDLConfigMissingPossibleValues);
            }

            newDisplayHint.DisplaySelectionText = this.propertyCells[CellIndexDescription.DisplaySelectionText];

            var displayFormats = string.IsNullOrEmpty(this.propertyCells[CellIndexDescription.DisplayFormat]) ? null : this.propertyCells[CellIndexDescription.DisplayFormat].Split(';');

            if (displayFormats != null)
            {
                foreach (var displayFormat in displayFormats)
                {
                    newDisplayHint.AddDisplayFormat(displayFormat);
                }
            }

            var displayExamples = string.IsNullOrEmpty(this.propertyCells[CellIndexDescription.DisplayExample]) ? null : this.propertyCells[CellIndexDescription.DisplayExample].Split(';');

            if (displayExamples != null)
            {
                foreach (var displayExample in displayExamples)
                {
                    newDisplayHint.AddDisplayExample(displayExample);
                }
            }

            newDisplayHint.ShowDisplayName = this.GetDisplayNameAttributeValue();

            if (!string.IsNullOrWhiteSpace(this.propertyCells[CellIndexDescription.ClientAction]))
            {
                string clientActionType = this.propertyCells[CellIndexDescription.ClientAction];
                ValidateDisplayHintActionType(clientActionType);
                newDisplayHint.Action = new DisplayHintAction(
                    this.propertyCells[CellIndexDescription.ClientAction],
                    this.GetIsDefaultAttributeValue(),
                    this.propertyCells[CellIndexDescription.Context],
                    this.GetNullableStringAttributeValue(CellIndexDescription.DestinationId));
            }

            string possibleValues = this.propertyCells[PropertyDisplayHintFactory.CellIndexDescription.PossibleValues];
            Dictionary<string, string[]> possibleValuesDictionary = string.IsNullOrEmpty(possibleValues) ? null : DisplayDescriptionStore.GetDictionaryFromConfigString(possibleValues, displayDictionaries);

            newDisplayHint.SetPossibleOptions(possibleValuesDictionary);

            return newDisplayHint;
        }

        private ButtonDisplayHint GetButtonDisplayHint()
        {
            string displayContent = this.propertyCells[CellIndexDescription.DisplayContent];

            if (string.IsNullOrEmpty(displayContent))
            {
                throw new PIDLConfigException(
                    this.configFileName,
                    this.configFileLineNumber,
                    "DisplayContent is a mandatory field for Button",
                    Constants.ErrorCodes.PIDLConfigFileRequiredColumnIsMissing);
            }

            ButtonDisplayHint newDisplayHint = new ButtonDisplayHint()
            {
                HintId = this.propertyCells[CellIndexDescription.PropertyHintId],
                IsHidden = this.GetIsHiddenAttributeValue(),
                IsDisabled = this.GetIsDisabledAttributeValue(),
                IsHighlighted = this.GetIsHighlightedAttributeValue(),
                IsBack = this.GetIsBackAttributeValue(),
                DisplayContent = this.propertyCells[CellIndexDescription.DisplayContent],
                DisplayContentDescription = string.IsNullOrEmpty(this.propertyCells[CellIndexDescription.DisplayContentDescription]) ? null : this.propertyCells[CellIndexDescription.DisplayContentDescription],
                DisplayCondition = this.GetDisplayConditionAttributeValue(),
                StyleHints = this.GetStyleHintsAttributeValue()
            };

            string clientActionType = this.propertyCells[CellIndexDescription.ClientAction];
            ValidateDisplayHintActionType(clientActionType);
            newDisplayHint.Action = string.IsNullOrEmpty(
                this.propertyCells[CellIndexDescription.ClientAction]) ?
                null : new DisplayHintAction(
                    this.propertyCells[CellIndexDescription.ClientAction],
                    this.GetIsDefaultAttributeValue(),
                    this.propertyCells[CellIndexDescription.Context],
                    this.GetNullableStringAttributeValue(CellIndexDescription.DestinationId));

            return newDisplayHint;
        }

        private HyperlinkDisplayHint GetHyperlinkDisplayHint()
        {
            string displayContent = this.propertyCells[CellIndexDescription.DisplayContent];

            if (string.IsNullOrEmpty(displayContent))
            {
                throw new PIDLConfigException(
                    this.configFileName,
                    this.configFileLineNumber,
                    "DisplayContent is a mandatory field for hyperlink",
                    Constants.ErrorCodes.PIDLConfigFileRequiredColumnIsMissing);
            }

            HyperlinkDisplayHint newDisplayHint = new HyperlinkDisplayHint()
            {
                HintId = this.propertyCells[CellIndexDescription.PropertyHintId],
                IsHidden = this.GetIsHiddenAttributeValue(),
                IsDisabled = this.GetIsDisabledAttributeValue(),
                IsHighlighted = this.GetIsHighlightedAttributeValue(),
                IsBack = this.GetIsBackAttributeValue(),
                DisplayContent = this.propertyCells[CellIndexDescription.DisplayContent],
                DisplayContentDescription = string.IsNullOrEmpty(this.propertyCells[CellIndexDescription.DisplayContentDescription]) ? null : this.propertyCells[CellIndexDescription.DisplayContentDescription],
                SourceUrl = string.IsNullOrEmpty(this.propertyCells[CellIndexDescription.SourceUrl]) ? null : this.propertyCells[CellIndexDescription.SourceUrl],
                DisplayImage = string.IsNullOrEmpty(this.propertyCells[CellIndexDescription.DisplayImage]) ? null : (Microsoft.Commerce.Payments.Common.Environments.Environment.IsProdOrPPEEnvironment ? "https://pmservices.cp.microsoft.com" : "https://pmservices.cp.microsoft-int.com") + this.propertyCells[CellIndexDescription.DisplayImage],
                DisplayHelpSequenceId = this.GetNullableStringAttributeValue(CellIndexDescription.DisplayHelpSequenceId),
                DisplayHelpSequenceText = this.GetNullableStringAttributeValue(CellIndexDescription.DisplayHelpSequenceText),
                DisplayCondition = this.GetDisplayConditionAttributeValue(),
                DisplayHelpPosition = this.GetNullableStringAttributeValue(CellIndexDescription.DisplayHelpPosition),
                StyleHints = this.GetStyleHintsAttributeValue()
            };

            string clientActionType = this.propertyCells[CellIndexDescription.ClientAction];
            ValidateDisplayHintActionType(clientActionType);
            newDisplayHint.Action = string.IsNullOrEmpty(
                this.propertyCells[CellIndexDescription.ClientAction]) ?
                null : new DisplayHintAction(
                    this.propertyCells[CellIndexDescription.ClientAction],
                    this.GetIsDefaultAttributeValue(),
                    this.propertyCells[CellIndexDescription.Context],
                    this.GetNullableStringAttributeValue(CellIndexDescription.DestinationId));

            return newDisplayHint;
        }

        private ExpressCheckoutButtonDisplayHint GetExpressCheckoutButtonDisplayHint(Dictionary<string, Dictionary<string, string[]>> displayDictionaries)
        {
            string sourceUrl = this.propertyCells[CellIndexDescription.SourceUrl];

            if (string.IsNullOrEmpty(sourceUrl))
            {
                throw new PIDLConfigException(
                    this.configFileName,
                    this.configFileLineNumber,
                    "SourceUrl is a mandatory field for ExpressCheckoutButton",
                    Constants.ErrorCodes.PIDLConfigFileRequiredColumnIsMissing);
            }

            ExpressCheckoutButtonDisplayHint newDisplayHint = new ExpressCheckoutButtonDisplayHint()
            {
                HintId = this.propertyCells[CellIndexDescription.PropertyHintId],
                IsHidden = this.GetIsHiddenAttributeValue(),
                IsDisabled = this.GetIsDisabledAttributeValue(),
                FrameName = this.propertyCells[CellIndexDescription.PropertyHintId],
                MessageTimeout = 3000,
                SourceUrl = string.IsNullOrEmpty(this.propertyCells[CellIndexDescription.SourceUrl]) ? null : (Microsoft.Commerce.Payments.Common.Environments.Environment.IsProdOrPPEEnvironment ? "https://pmservices.cp.microsoft.com" : "https://pmservices.cp.microsoft-int.com") + this.propertyCells[CellIndexDescription.SourceUrl],
                StyleHints = this.GetStyleHintsAttributeValue(),
            };

            return newDisplayHint;
        }

        private string GetInputScopeAttribute()
        {
            if (!string.IsNullOrWhiteSpace(this.propertyCells[CellIndexDescription.InputScope]))
            {
                InputScope inputScopeType;
                if (!Enum.TryParse<InputScope>(this.propertyCells[CellIndexDescription.InputScope].ToUpper(), out inputScopeType))
                {
                    throw new PIDLConfigException(
                        this.configFileName,
                        this.configFileLineNumber,
                        string.Format("InputScope Column is malformed. Unexpected value {0} found", this.propertyCells[CellIndexDescription.InputScope]),
                        Constants.ErrorCodes.PIDLConfigFileColumnIsMalformed);
                }
                else
                {
                    return Helper.TryToLower(inputScopeType.ToString());
                }
            }
            else
            {
                return null;
            }
        }

        private bool? GetIsHiddenAttributeValue()
        {
            return this.GetNullableBooleanAttributeValue("IsHidden", CellIndexDescription.IsHidden);
        }

        private bool? GetIsDisabledAttributeValue()
        {
            return this.GetNullableBooleanAttributeValue("IsDisabled", CellIndexDescription.IsDisabled);
        }

        private bool? GetIsHighlightedAttributeValue()
        {
            return this.GetNullableBooleanAttributeValue("IsHighlighted", CellIndexDescription.IsHighlighted);
        }

        private bool? GetIsDefaultAttributeValue()
        {
            return this.GetNullableBooleanAttributeValue("IsDefault", CellIndexDescription.IsDefault);
        }

        private bool? GetIsBackAttributeValue()
        {
            return this.GetNullableBooleanAttributeValue("IsBack", CellIndexDescription.IsBack);
        }

        private bool? GetIsSelectFirstItemAttributeValue()
        {
            return this.GetNullableBooleanAttributeValue("IsSelectFirstItem", CellIndexDescription.IsSelectFirstItem);
        }

        private bool? GetMaskInputAttributeValue()
        {
            return this.GetNullableBooleanAttributeValue("MaskInput", CellIndexDescription.MaskInput);
        }

        private bool? GetMaskDisplayAttributeValue()
        {
            return this.GetNullableBooleanAttributeValue("MaskDisplay", CellIndexDescription.MaskDisplay);
        }

        private IEnumerable<string> GetStyleHintsAttributeValue()
        {
            return PidlFactoryHelper.ParseStyleHints(this.propertyCells[CellIndexDescription.StyleHints]);
        }

        private bool? GetNullableBooleanAttributeValue(string attributeName, int cellIndexForAttribute)
        {
            string attributeValue = this.propertyCells[cellIndexForAttribute];
            if (string.IsNullOrEmpty(attributeValue))
            {
                return null;
            }
            else
            {
                bool result;
                if (bool.TryParse(attributeValue, out result))
                {
                    return result;
                }
                else
                {
                    throw new PIDLConfigException(
                        this.configFileName,
                        this.configFileLineNumber,
                        string.Format("The {0} attribute is not configured correctly. It should be either 'TRUE' OR 'FALSE'", attributeName),
                        Constants.ErrorCodes.PIDLConfigFileColumnIsMalformed);
                }
            }
        }

        private int? GetNullableIntegerAttributeValue(int cellIndexForAttribute)
        {
            string attributeValue = this.propertyCells[cellIndexForAttribute];

            int intValue;
            if (int.TryParse(attributeValue, out intValue))
            {
                return intValue;
            }
            else
            {
                return null;
            }
        }

        private string GetNullableStringAttributeValue(int cellIndexForAttribute)
        {
            return string.IsNullOrEmpty(this.propertyCells[cellIndexForAttribute]) ? null : this.propertyCells[cellIndexForAttribute];
        }

        private string GetDisplayNameAttributeValue()
        {
            if (!string.IsNullOrWhiteSpace(this.propertyCells[CellIndexDescription.ShowDisplayName]))
            {
                ShowDisplayNameOption showDisplayNameOption;
                if (!Enum.TryParse<ShowDisplayNameOption>(this.propertyCells[CellIndexDescription.ShowDisplayName].ToUpper(), out showDisplayNameOption))
                {
                    throw new PIDLConfigException(
                        this.configFileName,
                         this.configFileLineNumber,
                         string.Format("ShowDisplayName Column is malformed. Unexpected value {0} found", this.propertyCells[CellIndexDescription.ShowDisplayName]),
                        Constants.ErrorCodes.PIDLConfigFileColumnIsMalformed);
                }
                else
                {
                    return showDisplayNameOption.ToString().ToLower();
                }
            }
            else
            {
                return null;
            }
        }

        private DisplayCondition GetDisplayConditionAttributeValue()
        {
            if (!string.IsNullOrWhiteSpace(this.propertyCells[CellIndexDescription.DisplayConditionFunctionName]))
            {
                return new DisplayCondition(this.propertyCells[CellIndexDescription.DisplayConditionFunctionName]);
            }
            else
            {
                return null;
            }
        }

        private DataCollectionFilterDescription GetDataCollectionFilterDescriptionAttributeValue()
        {
            if (!string.IsNullOrWhiteSpace(this.propertyCells[CellIndexDescription.DataCollectionFilterFunctionName]))
            {
                return new DataCollectionFilterDescription(this.propertyCells[CellIndexDescription.DataCollectionFilterFunctionName]);
            }
            else
            {
                return null;
            }
        }

        private void ValidateDisplayType()
        {
            bool found = false;
            var values = Enum.GetValues(typeof(HintType));
            foreach (var val in values)
            {
                if (string.Equals(val.ToString(), this.propertyCells[CellIndexDescription.DisplayType], StringComparison.InvariantCultureIgnoreCase))
                {
                    found = true;
                }
            }

            if (!found)
            {
                throw new PIDLException(string.Format("The DisplayType has unexpected value : {0}", this.propertyCells[CellIndexDescription.DisplayType]), Constants.ErrorCodes.PIDLArgumentTypeIsInvalid);
            }
        }

        /// <summary>
        /// The index descriptions for the cells in the csv
        /// </summary>
        public static class CellIndexDescription
        {
            public const int PropertyHintId = 0;
            public const int CountryId = 1;
            public const int FeatureName = 2;
            public const int DisplayType = 3;
            public const int PropertyName = 4;
            public const int DependentPropertyName = 5;
            public const int DependentPropertyValueRegex = 6;
            public const int DisplayName = 7;
            public const int DisplayDescription = 8;
            public const int IsHidden = 9;
            public const int IsDisabled = 10;
            public const int IsHighlighted = 11;
            public const int IsDefault = 12;
            public const int IsBack = 13;
            public const int InputScope = 14;
            public const int MaskInput = 15;
            public const int ShowDisplayName = 16;
            public const int DisplayFormat = 17;
            public const int DisplayExample = 18;
            public const int MinSize = 19;
            public const int MaxSize = 20;
            public const int DisplayContent = 21;
            public const int DisplayContentDescription = 22;
            public const int DisplayHelpSequenceId = 23;
            public const int DisplayHelpSequenceText = 24;
            public const int ClientAction = 25;
            public const int Context = 26;
            public const int DestinationId = 27;
            public const int PossibleValues = 28;
            public const int DisplaySelectionText = 29;
            public const int SourceUrl = 30;
            public const int DisplayLogo = 31;
            public const int SelectType = 32;
            public const int MaskDisplay = 33;
            public const int DisplayConditionFunctionName = 34;
            public const int DataCollectionSource = 35;
            public const int DataCollectionFilterFunctionName = 36;
            public const int IsSelectFirstItem = 37;
            public const int DisplayImage = 38;
            public const int DisplayHelpPosition = 39;
            public const int StyleHints = 40;
        }
    }
}
