// <copyright file="Logger.cs" company="Microsoft">Copyright (c) Microsoft 2014. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.Tools.InstallCertificates
{
    using System;

    public static class Logger
    {
        // Used to prefix output from this tool.  This helps distinguis output from this tool vs. others
        // especially when viewed in AutoPilot logs which contains logs from maultiple processes.
        private const string AppName = "InstallCertificates";
        private static string indent = string.Empty;

        public static void Log(string fmt, params object[] args)
        {
            Console.WriteLine(string.Format("{0}: {1}", AppName, indent) + fmt, args);
        }

        public static void LogBegin(string functionName)
        {
            Log("Begin {0}", functionName);
            indent += "  ";
        }

        public static void LogEnd(string functionName)
        {
            if (indent.Length >= 2)
            {
                indent = indent.Substring(2);
            }

            Log("End {0}", functionName);
        }
    }
}
