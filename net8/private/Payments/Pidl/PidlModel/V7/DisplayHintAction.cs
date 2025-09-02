// <copyright file="DisplayHintAction.cs" company="Microsoft">Copyright (c) Microsoft 2015. All rights reserved.</copyright>
namespace Microsoft.Commerce.Payments.PidlModel.V7
{
    using System.Collections.Generic;

    using Newtonsoft.Json;

    public enum DisplayHintActionType
    {
        moveNext,
        movePrevious,
        moveFirst,
        moveLast,
        submit,
        navigate,
        gohome,
        delete,
        call,
        restAction,
        success,
        restartFlow,
        successWithPidlPayload,
        redirect,
        partnerAction,
        navigateAndMoveNext,
        poll,
        validate,
        moveNextAndPoll,
        updatePollAndMoveLast,
        handleFailure,
        updatePoll,
        successWithSelectedPidlAction,
        mergeData,
        propertyBindingAction,
        closeModalDialog,
        continueSuspendedAction,
        closePidlPage,
        moveToPageIndex,
        triggerEvent,
        triggerSubmit,
        partnerActionWithPidlPayload,
        postMessageToChildIFrames
    }

    /// <summary>
    /// Needed in controls to attach custom event names decoupled from their Display Hint
    /// </summary>
    public enum CustomEventNameType
    {
        viewTermsTriggered
    }

    /// <summary>
    /// A class for describing a generic Action associated wuth a Display hint
    /// </summary>
    public class DisplayHintAction
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DisplayHintAction"/> class.
        /// </summary>
        public DisplayHintAction() 
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DisplayHintAction"/> class.
        /// </summary>
        /// <param name="actionType">The Action Type that the Display hint represents</param>
        public DisplayHintAction(string actionType)
            : this(actionType, false, null, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DisplayHintAction"/> class with a given Context
        /// </summary>
        /// <param name="actionType">The Action Type that the Display hint represents</param>
        /// <param name="isDefault">To indicate whether it's default action or not</param>
        /// <param name="context">The context of the client action</param>
        /// <param name="destinationId">Identity of the client action for redirection</param>
        public DisplayHintAction(string actionType, bool? isDefault, object context, string destinationId)
        {
            this.ActionType = actionType;
            this.IsDefault = isDefault;
            this.Context = context;
            this.DestinationId = destinationId;
        }

        [JsonProperty(PropertyName = "type")]
        public string ActionType { get; set; }

        [JsonProperty(PropertyName = "isDefault")]
        public bool? IsDefault { get; set; }

        [JsonProperty(PropertyName = "context")]
        public object Context { get; set; }

        [JsonProperty(PropertyName = "context2")]
        public object Context2 { get; set; }

        [JsonProperty(PropertyName = "nextAction")]
        public DisplayHintAction NextAction { get; set; }

        [JsonProperty(PropertyName = "dest")]
        public string DestinationId { get; set; }
    }
}