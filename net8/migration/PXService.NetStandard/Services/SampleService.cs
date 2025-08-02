using CoreWCF;

namespace PXService.NetStandard.Services;

[ServiceContract]
public interface ISampleService
{
    [OperationContract]
    string Echo(string text);
}

public class SampleService : ISampleService
{
    public string Echo(string text) => $"Echo: {text}";
}
