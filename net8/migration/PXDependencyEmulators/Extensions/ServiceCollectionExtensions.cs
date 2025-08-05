namespace Microsoft.Commerce.Payments.Tests.Emulators.PXDependencyEmulators
{
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.DependencyInjection.Extensions;

    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection RegisterScenarioManager(this IServiceCollection services, string key, IScenarioManager manager)
        {
            services.TryAddSingleton<ScenarioManagerRegistry>();
            services.TryAddSingleton<IScenarioManagerRegistry>(sp => sp.GetRequiredService<ScenarioManagerRegistry>());

            services.AddSingleton(sp =>
            {
                var registry = sp.GetRequiredService<ScenarioManagerRegistry>();
                registry.RegisterScenarioManager(key, manager);
                return manager;
            });

            return services;
        }
    }
}
