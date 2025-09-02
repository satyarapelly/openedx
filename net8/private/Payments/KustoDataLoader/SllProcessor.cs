// <copyright file="SllProcessor.cs" company="Microsoft">Copyright (c) Microsoft 2018. All rights reserved.</copyright>

namespace KustoDataLoader
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using Kusto.Data.Common;
    using Microsoft.Commerce.Payments.Monitoring.KustoDataLoaderCore;

    internal class SllProcessor : FileProcessor
    {
        public SllProcessor(PXKustoDataLoaderSettings settings, string watchFolder, string fileWatchMask = "*.log")
            : base(
                  settings, 
                  Path.Combine(watchFolder, fileWatchMask),
                  Path.Combine(settings.DataDirectory, "KustoDataLoaderSllCursorInformation.csv"))
        {
            this.PXSettings = settings;
        }

        private PXKustoDataLoaderSettings PXSettings { get; set; }

        protected override IList<JsonColumnMapping> GetJsonMapping()
        {
            List<JsonColumnMapping> mapping = new List<JsonColumnMapping>()
            {
                new JsonColumnMapping() { ColumnName = "Timestamp", JsonPath = "$.time" },
                new JsonColumnMapping() { ColumnName = "EventName", JsonPath = "$.name" },
                new JsonColumnMapping() { ColumnName = "Level", JsonPath = "$.ext.sll.level" },
                new JsonColumnMapping() { ColumnName = "Environment", JsonPath = this.PXSettings.Environment },
                new JsonColumnMapping() { ColumnName = "MachineFunction", JsonPath = this.PXSettings.MachineFunction },
                new JsonColumnMapping() { ColumnName = "MachineName", JsonPath = Environment.MachineName },
                new JsonColumnMapping() { ColumnName = "Component", JsonPath = "$.data.ServiceName" },
                new JsonColumnMapping() { ColumnName = "ComponentEventName", JsonPath = "$.data.baseData.operationName" },
                new JsonColumnMapping() { ColumnName = "ActivityId", JsonPath = "$.data.ServerTraceId" },
                new JsonColumnMapping() { ColumnName = "RelatedActivityId", JsonPath = "$.data.RequestTraceId" },
                new JsonColumnMapping() { ColumnName = "CV", JsonPath = "$.cV" },
                new JsonColumnMapping() { ColumnName = "Message", JsonPath = "$.data.Message" },
                new JsonColumnMapping() { ColumnName = "Parameters", JsonPath = "$.data" },
            };

            return mapping;
        }

        protected override ProcessingStatus ParseFile(string fileName, StreamCursorItem cursorItem, StreamWriter writer)
        {
            var processingStatus = new ProcessingStatus();

            using (TextReader t = new StreamReader(File.Open(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)))
            {
                string line;
                while ((line = t.ReadLine()) != null)
                {
                    processingStatus.ProcessedEvents++;
                    if (processingStatus.ProcessedEvents <= cursorItem.ItemsProcessed)
                    {
                        continue;
                    }

                    if (ShouldFilter(line))
                    {
                        processingStatus.FilteredEvents++;
                        continue;
                    }

                    try
                    {
                        writer.WriteLine(line);
                        processingStatus.AddedEvents++;
                    }
                    catch (Exception e)
                    {
                        Trace.TraceError($"Sll Parser problem: { e.Message }. File { fileName }. Line: { processingStatus.ProcessedEvents }.");
                    }
                }
            }

            Trace.WriteLine($"{ DateTime.UtcNow.ToString("o") }| Items skipped: {cursorItem.ItemsProcessed} filtered: { processingStatus.FilteredEvents } added: { processingStatus.ProcessedEvents - cursorItem.ItemsProcessed - processingStatus.FilteredEvents }.");
            cursorItem.ItemsProcessed = processingStatus.ProcessedEvents;
            return processingStatus;
        }

        private static bool ShouldFilter(string line)
        {
            return !(line.Contains("\"name\":\"Microsoft.Commerce.Tracing.Sll.PXServiceIncomingOperation\"")
                || line.Contains("\"name\":\"Microsoft.Commerce.Tracing.Sll.PXServiceOutgoingOperation\"")
                || line.Contains("\"name\":\"Microsoft.Commerce.Tracing.Sll.PXServiceIntegrationError\"")
                || line.Contains("\"EventName\":\"PXServiceTraceException\""));
        }
    }
}
