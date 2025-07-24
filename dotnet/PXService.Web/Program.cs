using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

var app = builder.Build();

app.MapGet("/probe", ([FromServices] IConfiguration config) =>
{
    var buildVersion = config["BuildVersion"] ?? "";
    return new ServiceStatus { Status = "Alive", BuildVersion = buildVersion };
});

app.Run();

public class ServiceStatus
{
    public string? Status { get; set; }
    public string? BuildVersion { get; set; }
}
