namespace Microsoft.Commerce.Payments.Tests.Emulators.PXDependencyEmulators.Extensions
{
    using System;
    using Microsoft.Extensions.DependencyInjection;
    
    public static class ServiceProviderExtensions
    {
        public static IScenarioManager GetTestScenarioManager(this IServiceProvider provider, string key)
        {
            var registry = provider.GetRequiredService<IScenarioManagerRegistry>();
            return registry.GetManager(key);
        }
    }
}
