// <copyright file="AutopilotTraceListener.cs" company="Microsoft">Copyright (c) Microsoft 2018. All rights reserved.</copyright>

namespace KustoDataLoader
{
    using System.Diagnostics;
    using Microsoft.Search.Autopilot;

    internal class AutopilotTraceListener : TraceListener
    {
        public AutopilotTraceListener()
        {
            if (!APRuntime.IsInitialized)
            {
                APRuntime.Initialize("ServiceConfig.ini");
            }
        }

        public override void Write(string message)
        {
            Logger.Log(message);
        }

        public override void WriteLine(string message)
        {
            Logger.Log(message);
        }

        public override void Flush()
        {
            Logger.Flush();
            base.Flush();
        }
    }
}
