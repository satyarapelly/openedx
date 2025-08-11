//-----------------------------------------------------------------------
// <copyright file="EventTraceActivity.cs" company="Microsoft">
//     Copyright (c) Microsoft Corp. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Commerce.Payments.Common.Tracing
{
    using System;
    using System.Threading;
    using Microsoft.CommonSchema.Services.Logging;
    using Newtonsoft.Json;

    /// <summary>
    /// Represents the correlation ActivityId for ETW traces
    /// </summary>
    public class EventTraceActivity
    {
        private const string ActivityIdFieldName = "activity_id";
        private const string CorrelationVectorPropertyName = "correlation_vector";

        // this field is passed as reference to native code. 
        [JsonProperty(ActivityIdFieldName, DefaultValueHandling = DefaultValueHandling.Ignore)]
        private Guid activityId;

        [JsonProperty(CorrelationVectorPropertyName, DefaultValueHandling = DefaultValueHandling.Ignore)]
        private CorrelationVector correlationVectorV4;

        /// <summary>
        /// Initializes a new instance of the EventTraceActivity class.
        /// </summary>
        public EventTraceActivity()
            : this(Guid.NewGuid())
        {
        }

        /// <summary>
        /// Initializes a new instance of the EventTraceActivity class.
        /// </summary>
        /// <param name="activityId">Correlation Id</param>
        public EventTraceActivity(Guid activityId)
        {
            this.ActivityId = activityId;
            this.CorrelationVectorV4 = new CorrelationVector();
        }

        /// <summary>
        /// Gets the current <see cref="T:EventTraceActivity"/> from the logical call context.
        /// </summary>
        public static EventTraceActivity Current
        {
            get
            {
                var data = Thread.GetData(Thread.GetNamedDataSlot(EventTraceActivity.Name));
                return data != null ? (EventTraceActivity)data : EventTraceActivity.Empty;
            }
        }

        public static string Name
        {
            get { return "EventTraceActivity"; }
        }

        public static EventTraceActivity Empty
        {
            get
            {
                EventTraceActivity activity = new EventTraceActivity(Guid.Empty);

                // Here we attach a new correlation vector. We specifically don't want this to be
                // a singleton because this vector will get incremented, and potentially reach
                // a point where it exceeds the character limit of the correlation vector.
                // It will also produce overlapping traces for unrelated events.
                // Because it isn't a singleton, we want to be able to tell them apart, so
                // we uniquify half of the vector.
                // Because the vector is attempting to carry on in the spirit of EventTraceActivity.Empty
                // we set the non unique half to all 0's.
                activity.CorrelationVectorV4 = CorrelationVector.Extend(
                    string.Format(
                        "{0}{1}.0",
                        Convert.ToBase64String(new byte[8], 0, 6),
                        Convert.ToBase64String(Guid.NewGuid().ToByteArray(), 0, 6)));

                return activity;
            }
        }

        /// <summary>
        /// Gets or sets the ActivityId.
        /// </summary>
        public Guid ActivityId
        {
            get
            {
                return this.activityId;
            }

            set
            {
                this.activityId = value;
            }
        }

        /// <summary>
        /// Gets or sets the CorrelationVectorV4.
        /// </summary>
        public CorrelationVector CorrelationVectorV4
        {
            get
            {
                return this.correlationVectorV4;
            }

            set
            {
                this.correlationVectorV4 = value;
            }
        }
    }
}
