namespace Microsoft.Commerce.Payments.Tests.Emulators.PXDependencyEmulators
{
    public interface IScenarioManagerRegistry
    {
        IScenarioManager GetManager(string key);
    }
}
