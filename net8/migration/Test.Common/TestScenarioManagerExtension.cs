// <copyright file="TestScenarioManagerExtension.cs" company="Microsoft">Copyright (c) Microsoft 2017. All rights reserved.</copyright>

using System;
using System.IO;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Test.Common
{
    public static class TestScenarioManagerExtensions
    {
        public static IServiceCollection AddTestScenarioManager(
            this IServiceCollection services,
            IHostEnvironment environment,
            string scenarioFolder,
            string defaultTestScenario)
        {
            string fullPath = ResolveScenarioFolderPath(environment, scenarioFolder);

            var manager = new TestScenarioManager(fullPath, defaultTestScenario);
            services.AddSingleton(manager);

            return services;
        }

        private static string ResolveScenarioFolderPath(IHostEnvironment env, string scenarioFolder)
        {
            string path = Path.Combine(env.ContentRootPath, scenarioFolder);

            if (!Directory.Exists(path))
            {
                // Fallback to /TestScenarios under executing directory
                path = Path.Combine(GetCurrentAssemblyPath(), "TestScenarios");
            }

            return path;
        }

        private static string GetCurrentAssemblyPath()
        {
            string codeBase = Assembly.GetExecutingAssembly().Location;
            return Path.GetDirectoryName(codeBase) ?? Directory.GetCurrentDirectory();
        }
    }
}
