namespace Microsoft.Commerce.Payments.Tests.Emulators.PXDependencyEmulators
{
    using System;
    using System.Collections.Concurrent;

    public class ScenarioManagerRegistry : IScenarioManagerRegistry
    {
        private readonly ConcurrentDictionary<string, IScenarioManager> _managers = new(StringComparer.OrdinalIgnoreCase);

        public IScenarioManager GetManager(string key)
        {
            if (_managers.TryGetValue(key, out var manager))
            {
                return manager;
            }

            throw new InvalidOperationException($"Scenario manager not registered for key '{key}'.");
        }

        public void RegisterScenarioManager(string key, IScenarioManager manager)
        {
            _managers[key] = manager;
        }
    }
}
