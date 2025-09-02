// <copyright file="Program.cs" company="Microsoft">Copyright (c) Microsoft 2013. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.Tools.FileCleaner
{
    using System;
    using System.IO;

    internal class Program
    {
        public static string RootDirectoryToPurgeFrom { get; set; }

        public static DateTime TimeStampToPurgeFrom { get; set; }

        public static void Main(string[] args)
        {
            // set defaults
            Program.RootDirectoryToPurgeFrom = @"D:\Data\Logs\local\STR\MSTestResults";
            Program.TimeStampToPurgeFrom = DateTime.Now.AddDays(-2); // 2 days

            if (!Program.ParseArgsAndSetParams(args))
            {
                PrintUsage();
                Console.WriteLine("Nothing to do, exiting....");

                return;
            }

            if (!Directory.Exists(RootDirectoryToPurgeFrom))
            {
                Console.WriteLine(
                    "The directory: {0} does not exists, please specify a valid directory to purge, check usage below -->");

                PrintUsage();
                Console.WriteLine("Nothing to do, exiting....");

                return;
            }

            Console.WriteLine("******************************************************");
            Console.WriteLine("Deleting all files older than: " + Program.TimeStampToPurgeFrom.ToString());

            Program.PurgeDirectory(Program.RootDirectoryToPurgeFrom);

            Console.WriteLine("Finished all work, exiting now...");
            Console.WriteLine("******************************************************");
        }

        // We are not doing any directory level locking here, so 
        // potentially a different process can write to this directory. 
        // We can set ACL to controll access to this directory but from
        // the purposes of how this application is being used, it is not
        // necessary to have locking implemented.
        // 
        // The only case where we will hit an exception is if we check
        // the directory is empty and are going to delete it and before
        // we delete some other process writes to the same dir, in this
        // scenario we hit a DirectoryNotEmpty() exception. This should 
        // be OK, we will cleanup in next run.
        public static void PurgeDirectory(string directoryPathToCheck)
        {
            Console.WriteLine("Checking files under directory path: {0}", directoryPathToCheck);

            string[] files = Directory.GetFiles(directoryPathToCheck);

            foreach (string file in files)
            {
                FileInfo finfo = new FileInfo(file);

                if (finfo.LastAccessTime < Program.TimeStampToPurgeFrom)
                {
                    finfo.Delete();
                }
            }

            // Recurse sub-directories, will do depth-first
            string[] subDirectories = Directory.GetDirectories(directoryPathToCheck);
            foreach (string subDirectory in subDirectories)
            {
                // Delete files older than the specified timestamp
                Program.PurgeDirectory(subDirectory);

                // Check if direcotry is empty, if so delete it
                if (Directory.GetFiles(subDirectory).Length == 0 &&
                    Directory.GetDirectories(subDirectory).Length == 0)
                {
                    Directory.Delete(subDirectory);
                }
            }
        }

        private static bool ParseArgsAndSetParams(string[] args)
        {
            Console.WriteLine("Number of args passed: {0}", args.Length);

            if (args.Length > 0)
            {
                if (string.Compare(args[0], "/?", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    // user just asking for usage, exit from here, usage will get printed
                    return false;
                }

                if (Directory.Exists(args[0]))
                {
                    Program.RootDirectoryToPurgeFrom = args[0];
                }
                else
                {
                    Console.WriteLine(
                        "Incorrect directory parameter was passed as an argument. The specified directory path: {0} does not exists.", args[0]);
                    return false;
                }
            }

            // Check if a timestamp was specified
            double days = 0;
            if (args.Length > 1)
            {
                if (double.TryParse(args[1], out days) && days > 0)
                {
                    Program.TimeStampToPurgeFrom = DateTime.Now.AddDays(-days);
                }
                else
                {
                    Console.WriteLine("Incorrect timestamp value was specified as a parameter. Must be > 0 and a valid integer.");
                    return false;
                }
            }

            return true;
        }

        private static void PrintUsage()
        {
            Console.WriteLine("******************************************************");
            Console.WriteLine("\nUsage info:");
            Console.WriteLine("FilePurgeUtility.exe <absolue/relative directory path> <number of minutes>");
            
            Console.WriteLine(" <dir path: Optional Parameter. This can be absolute path or relative path\n" + 
                              " Default path is: D:\\app\\logs\\local\\MSTestResults");

            Console.WriteLine(" <# of days: Optional Paramater. Files which were last accessed < this # of days\n" +
                              " will be deleted.Default is 2 days, can be fractional");
            
            Console.WriteLine(" if you need to specify <# of days> you must also specify <dierctory path>\n");
            Console.WriteLine("******************************************************");
        }
    }
}