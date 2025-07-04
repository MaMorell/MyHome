using Microsoft.AspNetCore.Mvc;
using MyHome.ApiService.Extensions;
using MyHome.ApiService.HostedServices;
using MyHome.Core.Interfaces;
using MyHome.Core.Models.Audit;
using MyHome.Core.Models.Entities;
using MyHome.ServiceDefaults;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire client integrations.
builder.AddServiceDefaults();

// Add services to the container.
builder.Services.AddProblemDetails();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddMemoryCache();

builder.Services.RegisterLocalDependencies(builder.Configuration);
builder.Services.AddHostedService<HouseAutomationHost>();
builder.Services.AddHostedService<EnergyConsumptionWatcherHost>();
builder.Services.AddHostedService<SmartHubHost>();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseExceptionHandler();
app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

EnergysupplierEndpoints.Map(app);
PriceThearsholdsEndpoints.Map(app);
DeviceSettingsEndpoints.Map(app);

app.MapGet("/wifisocket/{name}/status", async ([FromServices] IWifiSocketsService service, string name) =>
{
    return await service.GetStatus(name);
});

app.MapGet("/auditevents", async ([FromServices] IRepository<AuditEvent> repository, [FromQuery] int limit) =>
{
    var result = await repository.GetAllAsync();
    if (result is null)
    {
        return [];
    }

    return result.OrderByDescending(r => r.Timestamp).Take(limit);
});

app.MapGet("/sensordata/{deviceName}", async ([FromServices] IRepository<SensorData> repository, [FromRoute] string deviceName, [FromQuery] int? limit) =>
{
    var result = await repository.GetAllAsync();
    if (result is null)
    {
        return [];
    }

    return result
        .Where(s => s.DeviceName == deviceName)
        .OrderByDescending(r => r.Timestamp)
        .Take(limit ?? 1000);
});

app.MapDefaultEndpoints();

app.Run();
