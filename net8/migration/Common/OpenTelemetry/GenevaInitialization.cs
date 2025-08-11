// <copyright file="GenevaInitialization.cs" company="Microsoft">Copyright (c) Microsoft 2025. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.Common.OpenTelemetry
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.Commerce.Payments.Common.Tracing;
    using Microsoft.Commerce.Payments.Common.Tracing.Metris.Dimensions;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// The class for initializing the geneva logging.
    /// </summary>
    public static class GenevaInitialization
    {
        /// <summary>
        /// The name of the Geneva log table.
        /// </summary>
        public const string LogTableName = "OpenTelemetryEvents";

        /// <summary>
        /// Custom fields for logging service.
        /// </summary>
        private static readonly string[] customFields = new string[] { "ActivityId", "RelatedActivityId", "CV", "Component", "ComponentEventName", "EventName", "Message", "Parameters" };

        public static string[] GetCustomFields()
        {
            return (string[])customFields.Clone();
        }

        /// <summary>
        /// Adding OpenTelemetry as a provider to the existing ASP.NET logging pipeline
        /// </summary>
        /// <param name="services">ServiceCollection object for storing initialized logging objects.</param>
        public static void SetupGenevaLogger(
            this IServiceCollection services)
        {
            services.AddLogging(builder =>
            {
                builder.ClearProviders();

                builder.AddOpenTelemetry(options =>
                {
                    options.AddGenevaLogExporter(exporterOptions =>
                    {
                        exporterOptions.ConnectionString = "EtwSession=OpenTelemetry";

                        // These will always be populated in every log from this instance.
                        exporterOptions.PrepopulatedFields = new Dictionary<string, object>()
                        {
                            [DefaultDimensions.RegionName.Key] = DefaultDimensions.RegionName.Value,
                            [DefaultDimensions.SiteName.Key] = DefaultDimensions.SiteName.Value,
                            [DefaultDimensions.SlotName.Key] = DefaultDimensions.SlotName.Value,
                            [DefaultDimensions.WebJobName.Key] = DefaultDimensions.WebJobName.Value,
                            [DefaultDimensions.MachineName.Key] = DefaultDimensions.MachineName.Value,
                            [DefaultDimensions.BuildVersion.Key] = DefaultDimensions.BuildVersion.Value,
                        };

                        // Mapping custom tables to match geneva config.
                        exporterOptions.TableNameMappings = new Dictionary<string, string>()
                        {
                            [LogTableName] = LogTableName,
                        };

                        // How to add extra columns to the schema.
                        exporterOptions.CustomFields = GetCustomFields();
                    });
                    options.IncludeFormattedMessage = true;
                });
            });

            // add native logger to service collection for use.
            ILoggerFactory loggerFactory = services.BuildServiceProvider().GetService<ILoggerFactory>();
            var logger = loggerFactory.CreateLogger(LogTableName);
            services.AddSingleton(logger);
            Logger.Initialize(OpenTelemetryServiceLogger.Create(logger), OpenTelemetryQosLogger.Create(logger));
        }
    }
}
