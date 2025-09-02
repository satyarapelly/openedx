// <copyright file="ITestRunner.cs" company="Microsoft">Copyright (c) Microsoft 2016. All rights reserved.</copyright>
namespace PidlTest
{
    using System.Threading.Tasks;

    internal interface ITestRunner
    {
        Task<int> StartTestAsync();

        Task StopTestAsync();

        bool ParseArguments(string[] args);
    }
}
