// <copyright file="ClientAction.cs" company="Microsoft">Copyright (c) Microsoft 2015. All rights reserved.</copyright>

namespace Tests.Common.Model.Pidl 
{
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    public enum ClientActionType
    {
        Pidl,
        Wait,
        Redirect,
        ReturnContext,
        ExecuteScriptAndResume,
        None,
        GoHome,
        Failure,
        RestAction,
        MergeData,
    }

    public class ClientAction
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ClientAction"/> class
        /// </summary>
        /// <param name="actionType">Type of Client Action</param>
        [JsonConstructor]
        public ClientAction(ClientActionType actionType)
            : this(actionType, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ClientAction"/> class with a given Context
        /// </summary>
        /// <param name="actionType">Type of Client Action</param>
        /// <param name="context">The context of the client action</param>
        public ClientAction(ClientActionType actionType, object context)
        {
            this.ActionType = actionType;
            this.Context = context;
        }

        [JsonConverter(typeof(StringEnumConverter))]
        [JsonProperty(PropertyName = "type")]
        public ClientActionType ActionType { get; private set; }

        [JsonProperty(PropertyName = "context")]
        public object Context { get; set; }

        [JsonProperty(PropertyName = "redirectPidl")]
        public object RedirectPidl { get; set; }

        [JsonProperty(PropertyName = "pidlRetainUserInput")]
        public bool? PidlRetainUserInput { get; set; }

        [JsonProperty(PropertyName = "pidlUserInputToClear")]
        public string PidlUserInputToClear { get; set; }

        [JsonProperty(PropertyName = "pidlError")]
        public object PidlError { get; set; }

        [JsonProperty(PropertyName = "nextAction")]
        public ClientAction NextAction { get; set; }
    }
}
