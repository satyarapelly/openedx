// <copyright file="HostingUtility.cs" company="Microsoft">Copyright (c) Microsoft 2016. All rights reserved.</copyright>
namespace PidlTest
{
    using Microsoft.Extensions.Configuration;
    using System;

    public static class AppConfig
    {
        public static IConfiguration Configuration { get; }

        static AppConfig()
        {
            Configuration = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();
        }
    }
}