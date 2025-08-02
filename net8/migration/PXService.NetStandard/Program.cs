using CoreWCF;
using CoreWCF.Configuration;
using PXService.NetStandard.Services;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSystemWebAdapters();
builder.Services.AddHttpForwarder();

builder.Services.AddServiceModelServices()
                .AddServiceModelConfigurationManagerFile("wcf.config");

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.UseSystemWebAdapters();

app.UseServiceModel(serviceBuilder =>
{
    serviceBuilder.AddService<SampleService>();
});

app.MapControllers();
app.MapForwarder("/{**catch-all}", app.Configuration["ProxyTo"]).Add(static builder => ((RouteEndpointBuilder)builder).Order = int.MaxValue);

app.Run();
