// <copyright file="DisplayHintDeserializer.cs" company="Microsoft">Copyright (c) Microsoft 2015. All rights reserved.</copyright>

namespace Tests.Common.Model
{
    using System;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using Pidl;

    public class DisplayHintDeserializer : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(DisplayHint);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var displayHintJObject = JObject.Load(reader);
            DisplayHint displayHint = CreateNewDisplayHint(displayHintJObject);
            serializer.Populate(displayHintJObject.CreateReader(), displayHint);
            return displayHint;
        }

        private static DisplayHint CreateNewDisplayHint(JObject displayHintJObject)
        {
            switch (displayHintJObject["displayType"].ToString())
            {
                case "property":
                    return new PropertyDisplayHint();
                case "secureproperty":
                    return new SecurePropertyDisplayHint();
                case "title":
                    return new TitleDisplayHint();
                case "heading":
                    return new HeadingDisplayHint();
                case "subheading":
                    return new SubheadingDisplayHint();
                case "text":
                    return new TextDisplayHint();
                case "button":
                    return new ButtonDisplayHint();
                case "hyperlink":
                    return new HyperlinkDisplayHint();
                case "image":
                    return new ImageDisplayHint();
                case "logo":
                    return new LogoDisplayHint();
                case "expression":
                    return new ExpressionDisplayHint();
                case "webView":
                    return new WebViewDisplayHint();
                case "pidlcontainer":
                    return new PidlContainerDisplayHint();
                case "group":
                    return new GroupDisplayHint();
                case "datacollectionbindinggroup":
                    return new GroupDisplayHint();
                case "textgroup":
                    return new TextGroupDisplayHint();
                case "page":
                    return new PageDisplayHint();
                case "prefillcontrol":
                    return new PrefillControlDisplayHint();
                case "iframe":
                    return new IFrameDisplayHint();
                case "separator":
                    return new SeparatorDisplayHint();
                case "audio":
                    return new AudioDisplayHint();
                case "captcha":
                    return new CaptchaDisplayHint();
                case "spinner":
                    return new SpinnerDisplayHint();
                case "pidlinstance":
                    return new PidlInstanceDisplayHint();
                case "expresscheckoutbutton":
                    return new ExpressCheckoutButtonDisplayHint();
                default:
                    return null;
            }
        }
    }
}