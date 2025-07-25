namespace Microsoft.Commerce.Tracing
{
    public class EventTraceActivity
    {
        public static readonly EventTraceActivity Empty = new EventTraceActivity();
    }
}

namespace Tracing
{
    public static class TraceCore
    {
        public static T TraceException<T>(Microsoft.Commerce.Tracing.EventTraceActivity activity, T exception) where T : System.Exception
        {
            return exception;
        }
        public static System.Exception TraceException(Microsoft.Commerce.Tracing.EventTraceActivity activity, System.Exception exception)
        {
            return exception;
        }
    }
}

namespace Common.Tracing
{
}

namespace Common.Environments
{
    public enum EnvironmentType
    {
        OneBox,
        Integration,
        Production
    }
}

namespace Common
{
    using System.Threading.Tasks;
    using Microsoft.Commerce.Tracing;
    using Common.Environments;

    public interface ISecretStore
    {
        Task<byte[]> ReadFileBytes(string file, EventTraceActivity traceActivityId);
    }

    public class Environment
    {
        public static Environment Current { get; } = new Environment();
        public EnvironmentType EnvironmentType { get; set; } = EnvironmentType.OneBox;
        public string EnvironmentName { get; set; } = "Development";
        public ISecretStore SecretStore { get; set; } = new FileSecretStore();

        private class FileSecretStore : ISecretStore
        {
            public Task<byte[]> ReadFileBytes(string file, EventTraceActivity traceActivityId) =>
                Task.FromResult(System.IO.File.ReadAllBytes(file));
        }
    }
}
