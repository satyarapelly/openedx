using Microsoft.ApplicationInsights.Extensibility;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Application Insights
builder.Services.AddApplicationInsightsTelemetry();

// Example HttpClient for calling external service
builder.Services.AddHttpClient<PXServiceNet8.Services.ExternalService>(client =>
{
    client.BaseAddress = new Uri("https://example.com/");
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();

app.MapControllers();

app.Run();
